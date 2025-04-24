using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;
using Dispatcher.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Dispatcher.Services;

public class ProductService : ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceBase
{
    private readonly NodeRegistry _nodeRegistry;

    public ProductService(NodeRegistry nodeRegistry)
    {
        _nodeRegistry = nodeRegistry;
    }

    public override async Task<ProductDto> GetProduct(ProductIdRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);

        return await client.GetProductAsync(request);
    }

    public override async Task<ProductDto> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Name);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);

        return await client.CreateProductAsync(request);
    }

    public override async Task<ProductDto> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);

        return await client.UpdateProductAsync(request);
    }

    public override async Task<DeleteProductResponse> DeleteProduct(ProductIdRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);

        return await client.DeleteProductAsync(request);
    }

    public override async Task<ProductList> ListProducts(Empty request, ServerCallContext context)
    {
        var tasks = _nodeRegistry.GetAllNodes().Select(async node =>
        {
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);
            return await client.ListProductsAsync(request);
        });

        var results = await Task.WhenAll(tasks);
        var combinedList = new ProductList();
        foreach (var result in results)
        {
            combinedList.Products.AddRange(result.Products);
        }

        return combinedList;
    }

    private NodeInfo GetTargetNode(string key)
    {
        var hash = Math.Abs(key.GetHashCode());
        var nodes = _nodeRegistry.GetAllNodes().ToList();
        if (!nodes.Any())
            throw new RpcException(new Status(StatusCode.Unavailable, "No available nodes"));

        return nodes[hash % nodes.Count];
    }
}