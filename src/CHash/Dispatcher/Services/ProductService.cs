using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using ProtosInterfaceDispatcher.Protos;
using Serilog;
using External = ProtosInterfaceDispatcher.Protos.External;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace Dispatcher.Services
{
    public class ProductService : External.ProductService.ProductServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;

        public ProductService(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }

        public override async Task<External.ProductDto> GetProduct(External.ProductIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            Log.Information("GetProduct({ProductId}) → node {NodeId}:{Port}", request.Id, node.NodeId, node.Port);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.ProductService.ProductServiceClient(channel);

            try
            {
                var internalResp = await client.GetProductAsync(new Internal.ProductIdRequest { Id = request.Id });
                return MapToExternal(internalResp);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to get product {ProductId}", request.Id);
                throw;
            }
        }

        public override async Task<External.ProductDto> CreateProduct(External.CreateProductRequest request, ServerCallContext context)
        {
            string idHex = HashUtils.ComputeSha256Id(request);
            var node = GetTargetNodeByKey(idHex);
            Log.Information("CreateProduct → ID {ProductId}, node {NodeId}:{Port}", idHex, node.NodeId, node.Port);

            var internalReq = new Internal.CreateProductRequestProxy
            {
                Id = idHex,
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.ProductService.ProductServiceClient(channel);

            try
            {
                var internalResp = await client.CreateProductAsync(internalReq);
                Log.Information("Product created: {ProductId}", internalResp.Id);
                return MapToExternal(internalResp);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to create product {ProductId}", idHex);
                throw;
            }
        }

        public override async Task<External.ProductDto> UpdateProduct(External.UpdateProductRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            Log.Information("UpdateProduct({ProductId}) → node {NodeId}:{Port}", request.Id, node.NodeId, node.Port);

            var internalReq = new Internal.UpdateProductRequest
            {
                Id = request.Id,
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.ProductService.ProductServiceClient(channel);

            try
            {
                var internalResp = await client.UpdateProductAsync(internalReq);
                return MapToExternal(internalResp);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to update product {ProductId}", request.Id);
                throw;
            }
        }

        public override async Task<External.DeleteProductResponse> DeleteProduct(External.ProductIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            Log.Information("DeleteProduct({ProductId}) → node {NodeId}:{Port}", request.Id, node.NodeId, node.Port);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.ProductService.ProductServiceClient(channel);

            try
            {
                var internalResp = await client.DeleteProductAsync(new Internal.ProductIdRequest { Id = request.Id });
                Log.Information("Product {ProductId} deleted: {Success}", request.Id, internalResp.Success);

                return new External.DeleteProductResponse { Success = internalResp.Success };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to delete product {ProductId}", request.Id);
                throw;
            }
        }

        public override async Task<External.ProductList> ListProducts(Empty request, ServerCallContext context)
        {
            Log.Information("ListProducts: querying all nodes");

            var tasks = _nodeRegistry.GetAllNodes().Select(async node =>
            {
                try
                {
                    using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
                    var client = new Internal.ProductService.ProductServiceClient(channel);
                    var response = await client.ListProductsAsync(request);
                    Log.Information("Node {NodeId}: received {Count} products", node.NodeId, response.Products.Count);
                    return response;
                }
                catch (RpcException ex)
                {
                    Log.Error(ex, "Failed to fetch products from node {NodeId}", node.NodeId);
                    return null;
                }
            });

            var responses = await Task.WhenAll(tasks);
            var result = new External.ProductList();

            foreach (var response in responses.Where(r => r != null))
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
                Log.Error("No available nodes for key {Key}", key);
                throw new RpcException(new Status(
                    StatusCode.Unavailable,
                    "No available hashing nodes to route the request"));
            }
        }

        private static External.ProductDto MapToExternal(Internal.ProductDto internalDto)
        {
            return new External.ProductDto
            {
                Id = internalDto.Id,
                Name = internalDto.Name,
                Price = internalDto.Price,
                StockQuantity = internalDto.StockQuantity
            };
        }
    }
}
