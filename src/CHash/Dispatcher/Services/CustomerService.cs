using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using ProtosInterfaceDispatcher.Protos;
using External = ProtosInterfaceDispatcher.Protos.External;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace Dispatcher.Services
{
    /// <summary>
    /// Прокси-сервис для CustomerService на диспетчере:
    /// принимает External запросы, маршрутит на ноды по Internal контракту,
    /// и возвращает External ответы.</summary>
    public class CustomerService : External.CustomerService.CustomerServiceBase
    {
        private readonly NodeRegistry _nodeRegistry;

        public CustomerService(NodeRegistry nodeRegistry)
        {
            _nodeRegistry = nodeRegistry;
        }

        public override async Task<External.CustomerDto> GetCustomer(
            External.CustomerIdRequest request,
            ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.CustomerService.CustomerServiceClient(channel);

            var internalReq  = new Internal.CustomerIdRequest { Id = request.Id };
            var internalResp = await client.GetCustomerAsync(internalReq);

            return new External.CustomerDto
            {
                Id          = internalResp.Id,
                FullName    = internalResp.FullName,
                Email       = internalResp.Email,
                PhoneNumber = internalResp.PhoneNumber,
                CreatedAt   = internalResp.CreatedAt
            };
        }

        public override async Task<External.CustomerDto> CreateCustomer(
            External.CreateCustomerRequest request,
            ServerCallContext context)
        {
            // 1) Генерируем детерминированный Id
            var keyHex = HashUtils.ComputeSha256Id(request);
            var node   = GetTargetNodeByKey(keyHex);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.CustomerService.CustomerServiceClient(channel);

            // 2) Формируем внутренний запрос с нашим Id
            var internalReq = new Internal.CreateCustomerRequest
            {
                Id          = keyHex,
                FullName    = request.FullName,
                Email       = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var internalResp = await client.CreateCustomerAsync(internalReq);

            return new External.CustomerDto
            {
                Id          = internalResp.Id,
                FullName    = internalResp.FullName,
                Email       = internalResp.Email,
                PhoneNumber = internalResp.PhoneNumber,
                CreatedAt   = internalResp.CreatedAt
            };
        }

        public override async Task<External.CustomerDto> UpdateCustomer(
            External.UpdateCustomerRequest request,
            ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.CustomerService.CustomerServiceClient(channel);

            var internalReq = new Internal.UpdateCustomerRequest
            {
                Id          = request.Id,
                FullName    = request.FullName,
                Email       = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var internalResp = await client.UpdateCustomerAsync(internalReq);

            return new External.CustomerDto
            {
                Id          = internalResp.Id,
                FullName    = internalResp.FullName,
                Email       = internalResp.Email,
                PhoneNumber = internalResp.PhoneNumber,
                CreatedAt   = internalResp.CreatedAt
            };
        }

        public override async Task<External.DeleteCustomerResponse> DeleteCustomer(
            External.CustomerIdRequest request,
            ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.CustomerService.CustomerServiceClient(channel);

            var internalReq  = new Internal.CustomerIdRequest { Id = request.Id };
            var internalResp = await client.DeleteCustomerAsync(internalReq);

            return new External.DeleteCustomerResponse { Success = internalResp.Success };
        }

        public override async Task<External.CustomerList> ListCustomers(
            Empty request,
            ServerCallContext context)
        {
            var externalList = new External.CustomerList();

            var tasks = _nodeRegistry.GetAllNodes().Select(async nodeInfo =>
            {
                using var ch  = GrpcChannel.ForAddress($"https://localhost:{nodeInfo.Port}");
                var cli       = new Internal.CustomerService.CustomerServiceClient(ch);
                return await cli.ListCustomersAsync(request);
            });

            var parts = await Task.WhenAll(tasks);
            foreach (var part in parts)
            {
                foreach (var dto in part.Customers)
                {
                    externalList.Customers.Add(new External.CustomerDto
                    {
                        Id          = dto.Id,
                        FullName    = dto.FullName,
                        Email       = dto.Email,
                        PhoneNumber = dto.PhoneNumber,
                        CreatedAt   = dto.CreatedAt
                    });
                }
            }

            return externalList;
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