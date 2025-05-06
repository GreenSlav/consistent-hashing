using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace HashingNode.Services
{
    /// <summary>
    /// Реализация gRPC-сервиса для работы с ордерами на hashing-ноде.
    /// Использует «Internal» контракты.
    /// </summary>
    public class OrderServiceImpl : Internal.OrderService.OrderServiceBase
    {
        // Хранилище внутренних DTO
        private static readonly ConcurrentDictionary<string, Internal.OrderDto> _ordersStorage 
            = new();

        /// <inheritdoc/>
        public override Task<Internal.OrderDto> CreateOrder(
            Internal.CreateOrderRequest request, 
            ServerCallContext context)
        {
            var newOrder = new Internal.OrderDto
            {
                Id          = request.Id,
                CustomerId  = request.CustomerId,
                OrderDate   = DateTime.UtcNow.ToString("O"),
                TotalAmount = request.TotalAmount
            };

            _ordersStorage[newOrder.Id] = newOrder;
            return Task.FromResult(newOrder);
        }

        /// <inheritdoc/>
        public override Task<Internal.OrderDto> GetOrder(
            Internal.OrderIdRequest request, 
            ServerCallContext context)
        {
            if (_ordersStorage.TryGetValue(request.Id, out var order))
            {
                return Task.FromResult(order);
            }

            throw new RpcException(
                new Status(StatusCode.NotFound, $"Order {request.Id} not found"));
        }

        /// <inheritdoc/>
        public override Task<Internal.OrderDto> UpdateOrder(
            Internal.UpdateOrderRequest request, 
            ServerCallContext context)
        {
            if (!_ordersStorage.ContainsKey(request.Id))
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, $"Order {request.Id} not found"));
            }

            var updated = new Internal.OrderDto
            {
                Id          = request.Id,
                CustomerId  = request.CustomerId,
                OrderDate   = request.OrderDate,
                TotalAmount = request.TotalAmount
            };

            _ordersStorage[request.Id] = updated;
            return Task.FromResult(updated);
        }

        /// <inheritdoc/>
        public override Task<Internal.DeleteOrderResponse> DeleteOrder(
            Internal.OrderIdRequest request, 
            ServerCallContext context)
        {
            if (_ordersStorage.TryRemove(request.Id, out _))
            {
                return Task.FromResult(new Internal.DeleteOrderResponse { Success = true });
            }

            throw new RpcException(
                new Status(StatusCode.NotFound, $"Order {request.Id} not found"));
        }

        /// <inheritdoc/>
        public override Task<Internal.OrderList> ListOrders(
            Empty request, 
            ServerCallContext context)
        {
            var list = new Internal.OrderList();
            list.Orders.AddRange(_ordersStorage.Values);
            return Task.FromResult(list);
        }
    }
}