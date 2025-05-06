using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using ProtosInterfaceDispatcher.Protos;
using External = ProtosInterfaceDispatcher.Protos.External;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace Dispatcher.Services
{
    public class ProductService : External.ProductService.ProductServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private readonly ILogger<ProductService> _logger;

        public ProductService(NodeRegistry nodeRegistry, ILogger<ProductService> logger)
        {
            _nodeRegistry = nodeRegistry;
            _logger = logger;
        }

        public override async Task<External.ProductDto> GetProduct(External.ProductIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.ProductService.ProductServiceClient(channel);

            var internalResp = await client.GetProductAsync(new Internal.ProductIdRequest { Id = request.Id });

            return MapToExternal(internalResp);
        }

        public override async Task<External.ProductDto> CreateProduct(External.CreateProductRequest request, ServerCallContext context)
        {
            string idHex = HashUtils.ComputeSha256Id(request);
            var node = GetTargetNodeByKey(idHex);

            var internalReq = new Internal.CreateProductRequestProxy()
            {
                Id            = idHex,
                Name          = request.Name,
                Price         = request.Price,
                StockQuantity = request.StockQuantity
            };

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.ProductService.ProductServiceClient(channel);

            var internalResp = await client.CreateProductAsync(internalReq);
            return MapToExternal(internalResp);
        }

        public override async Task<External.ProductDto> UpdateProduct(External.UpdateProductRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);

            var internalReq = new Internal.UpdateProductRequest
            {
                Id            = request.Id,
                Name          = request.Name,
                Price         = request.Price,
                StockQuantity = request.StockQuantity
            };

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.ProductService.ProductServiceClient(channel);

            var internalResp = await client.UpdateProductAsync(internalReq);
            return MapToExternal(internalResp);
        }

        public override async Task<External.DeleteProductResponse> DeleteProduct(External.ProductIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.ProductService.ProductServiceClient(channel);

            var internalResp = await client.DeleteProductAsync(new Internal.ProductIdRequest { Id = request.Id });

            return new External.DeleteProductResponse { Success = internalResp.Success };
        }

        public override async Task<External.ProductList> ListProducts(Empty request, ServerCallContext context)
        {
            var tasks = _nodeRegistry.GetAllNodes().Select(async node =>
            {
                using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
                var client = new Internal.ProductService.ProductServiceClient(channel);
                return await client.ListProductsAsync(request);
            });

            var responses = await Task.WhenAll(tasks);

            var result = new External.ProductList();
            foreach (var response in responses)
            {
                result.Products.AddRange(response.Products.Select(MapToExternal));
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

        private static External.ProductDto MapToExternal(Internal.ProductDto internalDto)
        {
            return new External.ProductDto
            {
                Id            = internalDto.Id,
                Name          = internalDto.Name,
                Price         = internalDto.Price,
                StockQuantity = internalDto.StockQuantity
            };
        }
    }
}
