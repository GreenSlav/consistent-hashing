using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Services
{
    public class ProductService : ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;
        private readonly ILogger<OrderService> _logger;

        public ProductService(NodeRegistry nodeRegistry, ILogger<OrderService> logger)
        {
            _nodeRegistry = nodeRegistry;
            _logger = logger;
        }

        public override async Task<ProductDto> GetProduct(ProductIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            _logger.LogInformation("\n\n\n!!!!!! Request received: {message}!!!!!!\n\n\n", node.Port);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);
            var response = await client.GetProductAsync(request);
            return response;
        }

        public override async Task<ProductDto> CreateProduct(CreateProductRequest request, ServerCallContext context)
        {
            // 1) Сгенерировать детерминированный ID
            string idHex = HashUtils.ComputeSha256Id(request);

            // 2) Выбрать ноду по этому же ключу
            var node = _nodeRegistry.GetNodeByKey(idHex);

            // 3) Отправить запрос Create на нужную ноду,
            //    но перед этим «перепаковать» request, установив Id:
            var proxiedReq = new CreateProductRequest
            {
                Id = idHex, // добавили поле Id в proto
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };
            
            _logger.LogInformation("\n\n\n!!!!!! Request received: {message}!!!!!!\n\n\n", node.Port);


            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);

            var response = await client.CreateProductAsync(proxiedReq).ResponseAsync;
            return response;
        }

        public override async Task<ProductDto> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);
            var response = await client.UpdateProductAsync(request);
            return response;
        }

        public override async Task<DeleteProductResponse> DeleteProduct(ProductIdRequest request,
            ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(channel);
            var response = await client.DeleteProductAsync(request);
            return response;
        }

        public override async Task<ProductList> ListProducts(Empty request, ServerCallContext context)
        {
            var tasks = _nodeRegistry
                .GetAllNodes()
                .Select(async n =>
                {
                    using var ch = GrpcChannel.ForAddress($"https://localhost:{n.Port}");
                    var cli = new ProtosInterfaceDispatcher.Protos.ProductService.ProductServiceClient(ch);
                    var response = await cli.ListProductsAsync(request);
                    return response;
                });

            var parts = await Task.WhenAll(tasks);
            var result = new ProductList();
            foreach (var part in parts)
            {
                result.Products.AddRange(part.Products);
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
    }
}