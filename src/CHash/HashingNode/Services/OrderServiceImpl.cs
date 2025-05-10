using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

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

        // Логгер через Serilog
        private static readonly ILogger _logger = Log.ForContext<OrderServiceImpl>();

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

            // Логируем создание заказа
            _logger.Information("Заказ {OrderId} создан для клиента {CustomerId}", newOrder.Id, newOrder.CustomerId);

            return Task.FromResult(newOrder);
        }

        /// <inheritdoc/>
        public override Task<Internal.OrderDto> GetOrder(
            Internal.OrderIdRequest request, 
            ServerCallContext context)
        {
            if (_ordersStorage.TryGetValue(request.Id, out var order))
            {
                // Заказ найден
                _logger.Debug("Запрос на получение заказа {OrderId}, найдено", request.Id);
                return Task.FromResult(order);
            }

            // Заказ не найден
            _logger.Warning("Заказ {OrderId} не найден при вызове метода GetOrder", request.Id);
            throw new RpcException(new Status(StatusCode.NotFound, $"Order {request.Id} not found"));
        }

        /// <inheritdoc/>
        public override Task<Internal.OrderDto> UpdateOrder(
            Internal.UpdateOrderRequest request, 
            ServerCallContext context)
        {
            if (!_ordersStorage.ContainsKey(request.Id))
            {
                // Ошибка обновления
                _logger.Warning("Попытка обновить несуществующий заказ {OrderId}", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Order {request.Id} not found"));
            }

            var updated = new Internal.OrderDto
            {
                Id          = request.Id,
                CustomerId  = request.CustomerId,
                OrderDate   = request.OrderDate,
                TotalAmount = request.TotalAmount
            };

            _ordersStorage[request.Id] = updated;

            // Успешное обновление
            _logger.Information("Заказ {OrderId} успешно обновлён", request.Id);

            return Task.FromResult(updated);
        }

        /// <inheritdoc/>
        public override Task<Internal.DeleteOrderResponse> DeleteOrder(
            Internal.OrderIdRequest request, 
            ServerCallContext context)
        {
            if (_ordersStorage.TryRemove(request.Id, out _))
            {
                // Успешное удаление
                _logger.Information("Заказ {OrderId} успешно удалён", request.Id);
                return Task.FromResult(new Internal.DeleteOrderResponse { Success = true });
            }

            // Попытка удалить несуществующий заказ
            _logger.Warning("Попытка удалить несуществующий заказ {OrderId}", request.Id);
            throw new RpcException(new Status(StatusCode.NotFound, $"Order {request.Id} not found"));
        }

        /// <inheritdoc/>
        public override Task<Internal.OrderList> ListOrders(
            Empty request, 
            ServerCallContext context)
        {
            var list = new Internal.OrderList();
            list.Orders.AddRange(_ordersStorage.Values);

            // Логируем количество
            _logger.Information("Запрошен список заказов. Всего: {Count}", list.Orders.Count);

            return Task.FromResult(list);
        }
    }
}