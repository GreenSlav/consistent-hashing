using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;
using Dispatcher.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Dispatcher.Services;

public class OrderService : ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceBase
{
    private readonly NodeRegistry _nodeRegistry;

    public OrderService(NodeRegistry nodeRegistry)
    {
        _nodeRegistry = nodeRegistry;
    }

    public override async Task<OrderDto> GetOrder(OrderIdRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);

        return await client.GetOrderAsync(request);
    }

    public override async Task<OrderDto> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.CustomerId);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);

        return await client.CreateOrderAsync(request);
    }

    public override async Task<OrderDto> UpdateOrder(UpdateOrderRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);

        return await client.UpdateOrderAsync(request);
    }

    public override async Task<DeleteOrderResponse> DeleteOrder(OrderIdRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);

        return await client.DeleteOrderAsync(request);
    }

    public override async Task<OrderList> ListOrders(Empty request, ServerCallContext context)
    {
        // Пример: отправляем запрос на все узлы и объединяем результаты
        var tasks = _nodeRegistry.GetAllNodes().Select(async node =>
        {
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceClient(channel);
            return await client.ListOrdersAsync(request);
        });

        var results = await Task.WhenAll(tasks);
        var combinedList = new OrderList();
        foreach (var result in results)
        {
            combinedList.Orders.AddRange(result.Orders);
        }

        return combinedList;
    }

    private NodeInfo GetTargetNode(string key)
    {
        // Простая хэш-функция для выбора узла
        // TODO: Изменить впоследствии хэш-функцию на более полезную
        var hash = Math.Abs(key.GetHashCode());
        var nodes = _nodeRegistry.GetAllNodes().ToList();
        if (!nodes.Any())
            throw new RpcException(new Status(StatusCode.Unavailable, "No available nodes"));

        return nodes[hash % nodes.Count];
    }
}