using Grpc.Core;
using ProtosInterfaceDispatcher.Protos;
using System.Collections.Concurrent;
using Google.Protobuf.WellKnownTypes;

namespace HashingNode.Services;

/// <summary>
/// Реализация gRPC-сервиса для управления продуктами.
/// Соответствует контракту, определённому в ProtosInterfaceDispatcher.Protos.
/// </summary>
public class ProductServiceImpl : ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceBase
{
    private static readonly ConcurrentDictionary<string, ProductDto> _productsStorage = new();

    /// <inheritdoc/>
    public override Task<ProductDto> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        var product = new ProductDto
        {
            Id = request.Id,
            Name = request.Name,
            Price = request.Price,
            StockQuantity = 0 // Инициализация при создании
        };

        _productsStorage[product.Id] = product;
        return Task.FromResult(product);
    }

    /// <inheritdoc/>
    public override Task<ProductDto> GetProduct(ProductIdRequest request, ServerCallContext context)
    {
        if (_productsStorage.TryGetValue(request.Id, out var product))
        {
            return Task.FromResult(product);
        }

        throw new RpcException(
            new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
    }

    /// <inheritdoc/>
    public override Task<ProductDto> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        if (!_productsStorage.TryGetValue(request.Id, out var existingProduct))
        {
            throw new RpcException(
                new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
        }

        var updatedProduct = new ProductDto
        {
            Id = request.Id,
            Name = request.Name,
            Price = request.Price,
            StockQuantity = existingProduct.StockQuantity // Сохранение текущего количества
        };

        _productsStorage[request.Id] = updatedProduct;
        return Task.FromResult(updatedProduct);
    }

    /// <inheritdoc/>
    public override Task<DeleteProductResponse> DeleteProduct(ProductIdRequest request, ServerCallContext context)
    {
        if (_productsStorage.TryRemove(request.Id, out _))
        {
            return Task.FromResult(new DeleteProductResponse { Success = true });
        }

        throw new RpcException(
            new Status(StatusCode.NotFound, $"Product {request.Id} not found"));
    }

    /// <inheritdoc/>
    public override Task<ProductList> ListProducts(Empty request, ServerCallContext context)
    {
        var response = new ProductList();
        response.Products.AddRange(_productsStorage.Values);
        return Task.FromResult(response);
    }
}