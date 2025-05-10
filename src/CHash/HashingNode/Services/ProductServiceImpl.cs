using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Serilog;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace HashingNode.Services
{
    /// <summary>
    /// Реализация gRPC-сервиса для работы с продуктами на hashing-ноде.
    /// Использует контракты из Internal (product_internal.proto).
    /// </summary>
    public class ProductServiceImpl : Internal.ProductService.ProductServiceBase
    {
        private static readonly ConcurrentDictionary<string, Internal.ProductDto> _productsStorage 
            = new();

        private static readonly Serilog.ILogger _logger = Log.ForContext<ProductServiceImpl>();

        /// <inheritdoc/>
        public override Task<Internal.ProductDto> CreateProduct(
            Internal.CreateProductRequestProxy request,
            ServerCallContext context)
        {
            var product = new Internal.ProductDto
            {
                Id = request.Id,
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            if (_productsStorage.TryAdd(product.Id, product))
            {
                _logger.Information("Продукт {ProductId} создан: {ProductName}, цена {Price}", product.Id, product.Name, product.Price);
            }
            else
            {
                _logger.Warning("Попытка создать продукт с уже существующим ID: {ProductId}", product.Id);
            }

            return Task.FromResult(product);
        }

        /// <inheritdoc/>
        public override Task<Internal.ProductDto> GetProduct(
            Internal.ProductIdRequest request,
            ServerCallContext context)
        {
            if (_productsStorage.TryGetValue(request.Id, out var product))
            {
                _logger.Debug("Запрос на получение продукта {ProductId}", request.Id);
                return Task.FromResult(product);
            }

            _logger.Warning("Продукт {ProductId} не найден", request.Id);
            throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
        }

        /// <inheritdoc/>
        public override Task<Internal.ProductDto> UpdateProduct(
            Internal.UpdateProductRequest request,
            ServerCallContext context)
        {
            if (!_productsStorage.TryGetValue(request.Id, out var existing))
            {
                _logger.Warning("Попытка обновить несуществующий продукт {ProductId}", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
            }

            var updated = new Internal.ProductDto
            {
                Id = request.Id,
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            _productsStorage[request.Id] = updated;
            _logger.Information("Продукт {ProductId} успешно обновлён", request.Id);

            return Task.FromResult(updated);
        }

        /// <inheritdoc/>
        public override Task<Internal.DeleteProductResponse> DeleteProduct(
            Internal.ProductIdRequest request,
            ServerCallContext context)
        {
            var success = _productsStorage.TryRemove(request.Id, out _);

            if (success)
            {
                _logger.Information("Продукт {ProductId} успешно удалён", request.Id);
            }
            else
            {
                _logger.Warning("Попытка удалить несуществующий продукт {ProductId}", request.Id);
            }

            return Task.FromResult(new Internal.DeleteProductResponse { Success = success });
        }

        /// <inheritdoc/>
        public override Task<Internal.ProductList> ListProducts(
            Empty request,
            ServerCallContext context)
        {
            var response = new Internal.ProductList();
            response.Products.AddRange(_productsStorage.Values);

            _logger.Information("Запрошено количество продуктов: {Count}", response.Products.Count);

            return Task.FromResult(response);
        }
    }
}