using System.Threading.Tasks;
using Dispatcher.Helpers;
using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using ProtosInterfaceDispatcher.Protos;

namespace Dispatcher.Services
{
    public class CustomerService : ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;

        public CustomerService(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }

        public override async Task<CustomerDto> GetCustomer(CustomerIdRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);
            var response = await client.GetCustomerAsync(request);
            return response;
        }

        public override async Task<CustomerDto> CreateCustomer(CreateCustomerRequest request, ServerCallContext context)
        {
            // 1) Сериализуем весь запрос в hex-строку SHA256
            var keyHex = HashUtils.ComputeSha256Id(request);
            // 2) Ищем по ней нужную ноду
            var node = GetTargetNodeByKey(keyHex);
            // 3) Проксируем запрос туда
            
            var proxied = new CreateCustomerRequest
            {
                Id          = keyHex,       // <-- и здесь
                FullName    = request.FullName,
                Email       = request.Email,
                PhoneNumber = request.PhoneNumber
            };
            
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);
            var response = await client.CreateCustomerAsync(proxied);
            return response;
        }

        public override async Task<CustomerDto> UpdateCustomer(UpdateCustomerRequest request, ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);
            var response = await client.UpdateCustomerAsync(request);
            return response;
        }

        public override async Task<DeleteCustomerResponse> DeleteCustomer(CustomerIdRequest request,
            ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);
            var response = await client.DeleteCustomerAsync(request);
            return response;
        }

        public override async Task<CustomerList> ListCustomers(Empty request, ServerCallContext context)
        {
            var response = new CustomerList();

            var tasks = _nodeRegistry
                .GetAllNodes()
                .Select(async nodeInfo =>
                {
                    using var ch = GrpcChannel.ForAddress($"https://localhost:{nodeInfo.Port}");
                    var cli = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(ch);
                    var responseLocal = await cli.ListCustomersAsync(request);
                    return responseLocal;
                });

            var results = await Task.WhenAll(tasks);
            foreach (var part in results)
            {
                response.Customers.AddRange(part.Customers);
            }

            return response;
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