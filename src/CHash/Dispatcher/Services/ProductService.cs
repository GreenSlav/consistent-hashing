using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using ProtosInterfaceDispatcher.Protos;
using ProtosInterfaceDispatcher.Protos.Internal;
using DeleteProductResponse = ProtosInterfaceDispatcher.Protos.Internal.DeleteProductResponse;
using ProductDto = ProtosInterfaceDispatcher.Protos.Internal.ProductDto;
using ProductIdRequest = ProtosInterfaceDispatcher.Protos.Internal.ProductIdRequest;
using ProductList = ProtosInterfaceDispatcher.Protos.Internal.ProductList;


namespace Dispatcher.Services
{
    public class ProductService : ProtosInterfaceDispatcher.Protos.Internal.ProductService.ProductServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private readonly ILogger<ProductService> _logger;

        public ProductService(NodeRegistry nodeRegistry, ILogger<ProductService> logger)
        {
            _nodeRegistry = nodeRegistry;
            _logger = logger;
        }

        public override async Task<ProductDto> GetProduct(ProductIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.Internal.ProductService.ProductServiceClient(channel); // <-- Internal.ProductService
            return await client.GetProductAsync(request);
        }

        public override async Task<ProductDto> CreateProduct(
            CreateProductRequestProxy request, ServerCallContext context)
        {
            string idHex = HashUtils.ComputeSha256Id(request);
            var node = _nodeRegistry.GetNodeByKey(idHex);

            var proxyReq = new CreateProductRequestProxy
            {
                Id = idHex,
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.Internal.ProductService.ProductServiceClient(channel);
    
            ProductDto internalResponse = await client.CreateProductAsync(proxyReq);
            return internalResponse;        }
        
        public override async Task<ProductDto> UpdateProduct(ProductDto request,
            ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.Internal.ProductService.ProductServiceClient(channel); // <-- Internal.ProductService
            return await client.UpdateProductAsync(request);
        }

        public override async Task<DeleteProductResponse> DeleteProduct(ProductIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.Internal.ProductService.ProductServiceClient(channel); // <-- Internal.ProductService
            return await client.DeleteProductAsync(request);
        }

        public override async Task<ProductList> ListProducts(Empty request, ServerCallContext context)
        {
            var tasks = _nodeRegistry.GetAllNodes().Select(async node =>
            {
                using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
                var client = new ProtosInterfaceDispatcher.Protos.Internal.ProductService.ProductServiceClient(channel); // <-- Internal.ProductService
                return await client.ListProductsAsync(request);
            });

            var responses = await Task.WhenAll(tasks);

            var result = new ProductList();
            foreach (var response in responses)
            {
                result.Products.AddRange(response.Products);
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
                throw new RpcException(new Status(
                    StatusCode.Unavailable,
                    "No available hashing nodes to route the request"));
            }
        }
        
        private static ProtosInterfaceDispatcher.Protos.ProductDto MapToExternal(ProductDto internalDto)
        {
            return new ProtosInterfaceDispatcher.Protos.ProductDto
            {
                Id = internalDto.Id,
                Name = internalDto.Name,
                Price = internalDto.Price,
                StockQuantity = internalDto.StockQuantity
            };
        }

    }
}
