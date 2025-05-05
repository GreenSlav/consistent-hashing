using Grpc.Core;
using ProtosInterfaceDispatcher.Protos.Internal;
using System.Collections.Concurrent;
using Google.Protobuf.WellKnownTypes;

namespace HashingNode.Services;

/// <summary>
/// Реализация gRPC-сервиса для управления продуктами.
/// Соответствует контракту, определённому в product_internal.proto.
/// </summary>
public class ProductServiceImpl : ProductService.ProductServiceBase
{
    private static readonly ConcurrentDictionary<string, ProductDto> _productsStorage = new();

    public override Task<ProductDto> CreateProduct(CreateProductRequestProxy request, ServerCallContext context)
    {
        var product = new ProductDto
        {
            Id = request.Id,
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        _productsStorage[product.Id] = product;
        return Task.FromResult(product);
    }

    public override Task<ProductDto> GetProduct(ProductIdRequest request, ServerCallContext context)
    {
        if (_productsStorage.TryGetValue(request.Id, out var product))
        {
            return Task.FromResult(product);
        }

        throw new RpcException(
            new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
    }

    public override Task<ProductDto> UpdateProduct(ProductDto request, ServerCallContext context)
    {
        if (!_productsStorage.ContainsKey(request.Id))
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
        }

        _productsStorage[request.Id] = request;
        return Task.FromResult(request);
    }

    public override Task<DeleteProductResponse> DeleteProduct(ProductIdRequest request, ServerCallContext context)
    {
        var success = _productsStorage.TryRemove(request.Id, out _);
        return Task.FromResult(new DeleteProductResponse { Success = success });
    }

    public override Task<ProductList> ListProducts(Empty request, ServerCallContext context)
    {
        var response = new ProductList();
        response.Products.AddRange(_productsStorage.Values);
        return Task.FromResult(response);
    }
}
