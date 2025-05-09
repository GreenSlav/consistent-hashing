using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using ProtosInterfaceDispatcher.Protos;
using Serilog;
using External = ProtosInterfaceDispatcher.Protos.External;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace Dispatcher.Services
{
    public class OrderService : External.OrderService.OrderServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;

        public OrderService(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }

        public override async Task<External.OrderDto> GetOrder(External.OrderIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            Log.Information("GetOrder({OrderId}) → node {NodeId}:{Port}", request.Id, node.NodeId, node.Port);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.OrderService.OrderServiceClient(channel);

            try
            {
                var internalReq = new Internal.OrderIdRequest { Id = request.Id };
                var internalResp = await client.GetOrderAsync(internalReq);

                return new External.OrderDto
                {
                    Id = internalResp.Id,
                    CustomerId = internalResp.CustomerId,
                    OrderDate = internalResp.OrderDate,
                    TotalAmount = internalResp.TotalAmount
                };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to get order {OrderId}", request.Id);
                throw;
            }
        }

        public override async Task<External.OrderDto> CreateOrder(External.CreateOrderRequest request, ServerCallContext context)
        {
            var keyHex = HashUtils.ComputeSha256Id(request);
            var node = GetTargetNodeByKey(keyHex);
            Log.Information("CreateOrder → ID {OrderId}, node {NodeId}:{Port}", keyHex, node.NodeId, node.Port);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.OrderService.OrderServiceClient(channel);

            var internalReq = new Internal.CreateOrderRequest
            {
                Id = keyHex,
                CustomerId = request.CustomerId,
                OrderDate = request.OrderDate,
                TotalAmount = request.TotalAmount
            };

            try
            {
                var internalResp = await client.CreateOrderAsync(internalReq);
                Log.Information("Order created: {OrderId}", internalResp.Id);

                return new External.OrderDto
                {
                    Id = internalResp.Id,
                    CustomerId = internalResp.CustomerId,
                    OrderDate = internalResp.OrderDate,
                    TotalAmount = internalResp.TotalAmount
                };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to create order {OrderId}", keyHex);
                throw;
            }
        }

        public override async Task<External.OrderDto> UpdateOrder(External.UpdateOrderRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            Log.Information("UpdateOrder({OrderId}) → node {NodeId}:{Port}", request.Id, node.NodeId, node.Port);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.OrderService.OrderServiceClient(channel);

            var internalReq = new Internal.UpdateOrderRequest
            {
                Id = request.Id,
                CustomerId = request.CustomerId,
                OrderDate = request.OrderDate,
                TotalAmount = request.TotalAmount
            };

            try
            {
                var internalResp = await client.UpdateOrderAsync(internalReq);

                return new External.OrderDto
                {
                    Id = internalResp.Id,
                    CustomerId = internalResp.CustomerId,
                    OrderDate = internalResp.OrderDate,
                    TotalAmount = internalResp.TotalAmount
                };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to update order {OrderId}", request.Id);
                throw;
            }
        }

        public override async Task<External.DeleteOrderResponse> DeleteOrder(External.OrderIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            Log.Information("DeleteOrder({OrderId}) → node {NodeId}:{Port}", request.Id, node.NodeId, node.Port);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.OrderService.OrderServiceClient(channel);

            try
            {
                var internalReq = new Internal.OrderIdRequest { Id = request.Id };
                var internalResp = await client.DeleteOrderAsync(internalReq);
                Log.Information("Order {OrderId} deleted: {Success}", request.Id, internalResp.Success);

                return new External.DeleteOrderResponse { Success = internalResp.Success };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to delete order {OrderId}", request.Id);
                throw;
            }
        }

        public override async Task<External.OrderList> ListOrders(Empty request, ServerCallContext context)
        {
            Log.Information("ListOrders: querying all nodes");

            var tasks = _nodeRegistry.GetAllNodes().Select(async n =>
            {
                try
                {
                    using var ch = GrpcChannel.ForAddress($"https://localhost:{n.Port}");
                    var cli = new Internal.OrderService.OrderServiceClient(ch);
                    var part = await cli.ListOrdersAsync(request);
                    Log.Information("Node {NodeId}: received {Count} orders", n.NodeId, part.Orders.Count);
                    return part;
                }
                catch (RpcException ex)
                {
                    Log.Error(ex, "Failed to fetch orders from node {NodeId}", n.NodeId);
                    return null;
                }
            });

            var parts = await Task.WhenAll(tasks);
            var result = new External.OrderList();

            foreach (var part in parts.Where(p => p != null))
            {
                result.Orders.AddRange(part.Orders.Select(o => new External.OrderDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    OrderDate = o.OrderDate,
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
                Log.Error("No available nodes for key {Key}", key);
                throw new RpcException(new Status(
                    StatusCode.Unavailable,
                    "No available hashing nodes to route the request"));
            }
        }
    }
}
