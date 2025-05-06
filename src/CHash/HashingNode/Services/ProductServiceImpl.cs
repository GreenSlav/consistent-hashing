using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace HashingNode.Services;

/// <summary>
/// Реализация gRPC-сервиса для работы с продуктами на hashing-ноде.
/// Использует контракты из Internal (product_internal.proto).
/// </summary>
public class ProductServiceImpl : Internal.ProductService.ProductServiceBase
{
    private static readonly ConcurrentDictionary<string, Internal.ProductDto> _productsStorage = new();

    /// <inheritdoc/>
    public override Task<Internal.ProductDto> CreateProduct(
        Internal.CreateProductRequestProxy request,
        ServerCallContext context)
    {
        var product = new Internal.ProductDto
        {
            Id             = request.Id,
            Name           = request.Name,
            Price          = request.Price,
            StockQuantity  = request.StockQuantity
        };

        _productsStorage[product.Id] = product;
        return Task.FromResult(product);
    }

    /// <inheritdoc/>
    public override Task<Internal.ProductDto> GetProduct(
        Internal.ProductIdRequest request,
        ServerCallContext context)
    {
        if (_productsStorage.TryGetValue(request.Id, out var product))
        {
            return Task.FromResult(product);
        }

        throw new RpcException(
            new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
    }

    /// <inheritdoc/>
    public override Task<Internal.ProductDto> UpdateProduct(
        Internal.UpdateProductRequest request,
        ServerCallContext context)
    {
        if (!_productsStorage.ContainsKey(request.Id))
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
        }

        var updated = new Internal.ProductDto
        {
            Id             = request.Id,
            Name           = request.Name,
            Price          = request.Price,
            StockQuantity  = request.StockQuantity
        };

        _productsStorage[request.Id] = updated;
        return Task.FromResult(updated);
    }

    /// <inheritdoc/>
    public override Task<Internal.DeleteProductResponse> DeleteProduct(
        Internal.ProductIdRequest request,
        ServerCallContext context)
    {
        var success = _productsStorage.TryRemove(request.Id, out _);
        return Task.FromResult(new Internal.DeleteProductResponse { Success = success });
    }

    /// <inheritdoc/>
    public override Task<Internal.ProductList> ListProducts(
        Empty request,
        ServerCallContext context)
    {
        var response = new Internal.ProductList();
        response.Products.AddRange(_productsStorage.Values);
        return Task.FromResult(response);
    }
}
