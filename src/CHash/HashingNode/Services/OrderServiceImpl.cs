using Grpc.Core;
using ProtosInterfaceDispatcher.Protos;
using System.Collections.Concurrent;
using Google.Protobuf.WellKnownTypes;

namespace HashingNode.Services;

/// <summary>
/// Реализация gRPC-сервиса для работы с ордерами.
/// </summary>
public class OrderServiceImpl : ProtosInterfaceDispatcher.Protos.OrderService.OrderServiceBase
{
    private static readonly ConcurrentDictionary<string, OrderDto> _ordersStorage = new();

    /// <inheritdoc/>
    /// <remarks>
    /// Создает новый ордер со следующими характеристиками:
    /// - Генерирует уникальный айдишник
    /// - Устанавливает текущую дату и время
    /// - Сохраняет переданные данные о клиенте и сумме ордера
    /// </remarks>
    public override Task<OrderDto> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        var newOrder = new OrderDto
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = request.CustomerId,
            OrderDate = DateTime.UtcNow.ToString("O"), // ISO 8601 extended format
            TotalAmount = request.TotalAmount
        };

        _ordersStorage[newOrder.Id] = newOrder;
        return Task.FromResult(newOrder);
    }

    /// <inheritdoc/>
    /// <exception cref="RpcException">
    /// Возникает с кодом StatusCode.NotFound (5) если заказ не найден
    /// </exception>
    public override Task<OrderDto> GetOrder(OrderIdRequest request, ServerCallContext context)
    {
        if (_ordersStorage.TryGetValue(request.Id, out var order))
        {
            return Task.FromResult(order);
        }

        throw new RpcException(
            new Status(StatusCode.NotFound, $"Order {request.Id} not found"),
            new Metadata { { "ErrorDetails", $"Order ID: {request.Id}" } });
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Полностью заменяет все данные заказа!!!!!!!!!!
    /// </remarks>
    /// <exception cref="RpcException">
    /// Возникает с кодом StatusCode.NotFound (5) если заказ не существует
    /// </exception>
    public override Task<OrderDto> UpdateOrder(UpdateOrderRequest request, ServerCallContext context)
    {
        if (!_ordersStorage.ContainsKey(request.Id))
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, $"Order {request.Id} not found"),
                new Metadata { { "Operation", "Update" } });
        }

        var updatedOrder = new OrderDto
        {
            Id = request.Id,
            CustomerId = request.CustomerId,
            OrderDate = request.OrderDate,
            TotalAmount = request.TotalAmount
        };

        _ordersStorage[request.Id] = updatedOrder;
        return Task.FromResult(updatedOrder);
    }

    /// <inheritdoc/>
    /// <returns>
    /// DeleteOrderResponse с полем Success = true при успешном удалении
    /// </returns>
    /// <exception cref="RpcException">
    /// Возникает с кодом StatusCode.NotFound (5) если заказ не существует
    /// </exception>
    public override Task<DeleteOrderResponse> DeleteOrder(OrderIdRequest request, ServerCallContext context)
    {
        if (_ordersStorage.TryRemove(request.Id, out _))
        {
            return Task.FromResult(new DeleteOrderResponse { Success = true });
        }

        throw new RpcException(
            new Status(StatusCode.NotFound, $"Order {request.Id} not found"),
            new Metadata { { "Operation", "Delete" } });
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Возвращает все, имеющиеся на текущей ноде.
    /// </remarks>
    public override Task<OrderList> ListOrders(Empty request, ServerCallContext context)
    {
        var response = new OrderList();
        response.Orders.AddRange(_ordersStorage.Values);
        return Task.FromResult(response);
    }
}