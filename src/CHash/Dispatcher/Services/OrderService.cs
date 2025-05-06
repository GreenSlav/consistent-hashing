using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using ProtosInterfaceDispatcher.Protos;
using External = ProtosInterfaceDispatcher.Protos.External;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace Dispatcher.Services
{
    public class OrderService : External.OrderService.OrderServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private readonly ILogger<OrderService> _logger;

        public OrderService(NodeRegistry nodeRegistry, ILogger<OrderService> logger)
        {
            _nodeRegistry = nodeRegistry;
            _logger = logger;
        }

        public override async Task<External.OrderDto> GetOrder(External.OrderIdRequest request, ServerCallContext context)
        {
            // Выбираем ноду по Id
            var node = GetTargetNodeByKey(request.Id);
            _logger.LogInformation("Routing GetOrder({OrderId}) to node on port {Port}", request.Id, node.Port);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.OrderService.OrderServiceClient(channel);

            // Проксируем запрос
            var internalReq = new Internal.OrderIdRequest { Id = request.Id };
            var internalResp = await client.GetOrderAsync(internalReq);

            // Конвертируем в внешний DTO
            return new External.OrderDto
            {
                Id          = internalResp.Id,
                CustomerId  = internalResp.CustomerId,
                OrderDate   = internalResp.OrderDate,
                TotalAmount = internalResp.TotalAmount
            };
        }

        public override async Task<External.OrderDto> CreateOrder(External.CreateOrderRequest request, ServerCallContext context)
        {
            // Генерируем детерминированный ID и выбираем ноду
            var keyHex = HashUtils.ComputeSha256Id(request);
            var node   = GetTargetNodeByKey(keyHex);
            _logger.LogInformation("Routing CreateOrder → node on port {Port}", node.Port);

            // Формируем внутренний запрос с нашим Id
            var internalReq = new Internal.CreateOrderRequest
            {
                Id          = keyHex,
                CustomerId  = request.CustomerId,
                OrderDate   = request.OrderDate,
                TotalAmount = request.TotalAmount
            };

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.OrderService.OrderServiceClient(channel);
            var internalResp = await client.CreateOrderAsync(internalReq);

            // Внешний ответ повторяет поля
            return new External.OrderDto
            {
                Id          = internalResp.Id,
                CustomerId  = internalResp.CustomerId,
                OrderDate   = internalResp.OrderDate,
                TotalAmount = internalResp.TotalAmount
            };
        }

        public override async Task<External.OrderDto> UpdateOrder(External.UpdateOrderRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            _logger.LogInformation("Routing UpdateOrder({OrderId}) to node on port {Port}", request.Id, node.Port);

            // Внутренний запрос
            var internalReq = new Internal.UpdateOrderRequest
            {
                Id          = request.Id,
                CustomerId  = request.CustomerId,
                OrderDate   = request.OrderDate,
                TotalAmount = request.TotalAmount
            };

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.OrderService.OrderServiceClient(channel);
            var internalResp = await client.UpdateOrderAsync(internalReq);

            return new External.OrderDto
            {
                Id          = internalResp.Id,
                CustomerId  = internalResp.CustomerId,
                OrderDate   = internalResp.OrderDate,
                TotalAmount = internalResp.TotalAmount
            };
        }

        public override async Task<External.DeleteOrderResponse> DeleteOrder(External.OrderIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            _logger.LogInformation("Routing DeleteOrder({OrderId}) to node on port {Port}", request.Id, node.Port);

            var internalReq = new Internal.OrderIdRequest { Id = request.Id };

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.OrderService.OrderServiceClient(channel);
            var internalResp = await client.DeleteOrderAsync(internalReq);

            return new External.DeleteOrderResponse { Success = internalResp.Success };
        }

        public override async Task<External.OrderList> ListOrders(Empty request, ServerCallContext context)
        {
            var tasks = _nodeRegistry.GetAllNodes().Select(async n =>
            {
                using var ch = GrpcChannel.ForAddress($"https://localhost:{n.Port}");
                var cli = new Internal.OrderService.OrderServiceClient(ch);
                return await cli.ListOrdersAsync(request);
            });

            var parts = await Task.WhenAll(tasks);
            var result = new External.OrderList();
            foreach (var part in parts)
            {
                // Конвертация каждого внутреннего OrderDto → внешний
                result.Orders.AddRange(part.Orders.Select(o => new External.OrderDto
                {
                    Id          = o.Id,
                    CustomerId  = o.CustomerId,
                    OrderDate   = o.OrderDate,
                    TotalAmount = o.TotalAmount
                }));
            }

            return result;
        }

        private NodeInfo GetTargetNodeByKey(string key)
        {
            try
            {
                return _nodeRegistry.GetNodeByKey(key);
            }
            catch (InvalidOperationException)
            {
                throw new RpcException(new Status(
                    StatusCode.Unavailable,
                    "No available hashing nodes to route the request"));
            }
        }
    }
}