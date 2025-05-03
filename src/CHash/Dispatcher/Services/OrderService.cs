using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Services
{
    public class OrderService : ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private readonly ILogger<OrderService> _logger;

        public OrderService(NodeRegistry nodeRegistry, ILogger<OrderService> logger)
        {
            _nodeRegistry = nodeRegistry;
            _logger = logger;
        }

        public override async Task<OrderDto> GetOrder(OrderIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            _logger.LogInformation("\n\n\n!!!!!! Request received: {message}!!!!!!\n\n\n", node.Port);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);
            var response = await client.GetOrderAsync(request);
            return response;
        }

        public override async Task<OrderDto> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            // 1) Сериализуем весь запрос в hex-строку SHA256
            var keyHex = HashUtils.ComputeSha256Id(request);
            // 2) Ищем по ней нужную ноду
            var node = GetTargetNodeByKey(keyHex);
            // 3) Проксируем запрос туда
            var proxied = new CreateOrderRequest
            {
                Id          = keyHex,
                CustomerId  = request.CustomerId,
                OrderDate   = request.OrderDate,
                TotalAmount = request.TotalAmount
            };

            _logger.LogInformation("\n\n\n!!!!!! Request received: {message}!!!!!!\n\n\n", node.Port);
            
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);
            var response = await client.CreateOrderAsync(proxied);
            return response;
        }

        public override async Task<OrderDto> UpdateOrder(UpdateOrderRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);
            var response = await client.UpdateOrderAsync(request);
            return response;
        }

        public override async Task<DeleteOrderResponse> DeleteOrder(OrderIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);
            var response = await client.DeleteOrderAsync(request);
            return response;
        }

        public override async Task<OrderList> ListOrders(Empty request, ServerCallContext context)
        {
            var tasks = _nodeRegistry
                .GetAllNodes()
                .Select(async n =>
                {
                    using var ch = GrpcChannel.ForAddress($"https://localhost:{n.Port}");
                    var cli = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(ch);
                    var response = await cli.ListOrdersAsync(request);
                    return response;
                });

            var parts = await Task.WhenAll(tasks);
            var result = new OrderList();
            foreach (var part in parts)
            {
                result.Orders.AddRange(part.Orders);
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