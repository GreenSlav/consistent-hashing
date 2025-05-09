using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;
using Dispatcher.Helpers;
using Serilog;
using ProtosInterfaceDispatcher.Protos;
using External = ProtosInterfaceDispatcher.Protos.External;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace Dispatcher.Services
{
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
            Log.Information("GetCustomer: routing to node {NodeId} at port {Port}", node.NodeId, node.Port);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.CustomerService.CustomerServiceClient(channel);

            try
            {
                var internalResp = await client.GetCustomerAsync(new Internal.CustomerIdRequest { Id = request.Id });

                return new External.CustomerDto
                {
                    Id = internalResp.Id,
                    FullName = internalResp.FullName,
                    Email = internalResp.Email,
                    PhoneNumber = internalResp.PhoneNumber,
                    CreatedAt = internalResp.CreatedAt
                };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to get customer with ID {Id}", request.Id);
                throw;
            }
        }

        public override async Task<External.CustomerDto> CreateCustomer(
            External.CreateCustomerRequest request,
            ServerCallContext context)
        {
            var keyHex = HashUtils.ComputeSha256Id(request);
            var node = GetTargetNodeByKey(keyHex);
            Log.Information("CreateCustomer: generated ID {Id}, routing to node {NodeId}", keyHex, node.NodeId);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.CustomerService.CustomerServiceClient(channel);

            var internalReq = new Internal.CreateCustomerRequest
            {
                Id = keyHex,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            try
            {
                var internalResp = await client.CreateCustomerAsync(internalReq);
                Log.Information("Customer created with ID {Id}", internalResp.Id);

                return new External.CustomerDto
                {
                    Id = internalResp.Id,
                    FullName = internalResp.FullName,
                    Email = internalResp.Email,
                    PhoneNumber = internalResp.PhoneNumber,
                    CreatedAt = internalResp.CreatedAt
                };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to create customer with ID {Id}", keyHex);
                throw;
            }
        }

        public override async Task<External.CustomerDto> UpdateCustomer(
            External.UpdateCustomerRequest request,
            ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            Log.Information("UpdateCustomer: routing to node {NodeId} for ID {Id}", node.NodeId, request.Id);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.CustomerService.CustomerServiceClient(channel);

            var internalReq = new Internal.UpdateCustomerRequest
            {
                Id = request.Id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            try
            {
                var internalResp = await client.UpdateCustomerAsync(internalReq);

                return new External.CustomerDto
                {
                    Id = internalResp.Id,
                    FullName = internalResp.FullName,
                    Email = internalResp.Email,
                    PhoneNumber = internalResp.PhoneNumber,
                    CreatedAt = internalResp.CreatedAt
                };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to update customer with ID {Id}", request.Id);
                throw;
            }
        }

        public override async Task<External.DeleteCustomerResponse> DeleteCustomer(
            External.CustomerIdRequest request,
            ServerCallContext context)
        {
            var node = GetTargetNodeByKey(request.Id);
            Log.Information("DeleteCustomer: routing to node {NodeId} for ID {Id}", node.NodeId, request.Id);

            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new Internal.CustomerService.CustomerServiceClient(channel);

            try
            {
                var internalResp = await client.DeleteCustomerAsync(new Internal.CustomerIdRequest { Id = request.Id });
                Log.Information("Customer {Id} deleted: {Success}", request.Id, internalResp.Success);

                return new External.DeleteCustomerResponse { Success = internalResp.Success };
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Failed to delete customer with ID {Id}", request.Id);
                throw;
            }
        }

        public override async Task<External.CustomerList> ListCustomers(Empty request, ServerCallContext context)
        {
            Log.Information("ListCustomers: requesting from all nodes");

            var externalList = new External.CustomerList();

            var tasks = _nodeRegistry.GetAllNodes().Select(async nodeInfo =>
            {
                try
                {
                    using var ch = GrpcChannel.ForAddress($"https://localhost:{nodeInfo.Port}");
                    var cli = new Internal.CustomerService.CustomerServiceClient(ch);
                    var part = await cli.ListCustomersAsync(request);
                    Log.Information("Fetched {Count} customers from node {NodeId}", part.Customers.Count, nodeInfo.NodeId);
                    return part;
                }
                catch (RpcException ex)
                {
                    Log.Error(ex, "Failed to fetch customers from node {NodeId}", nodeInfo.NodeId);
                    return null;
                }
            });

            var parts = await Task.WhenAll(tasks);
            foreach (var part in parts.Where(p => p != null))
            {
                foreach (var dto in part.Customers)
                {
                    externalList.Customers.Add(new External.CustomerDto
                    {
                        Id = dto.Id,
                        FullName = dto.FullName,
                        Email = dto.Email,
                        PhoneNumber = dto.PhoneNumber,
                        CreatedAt = dto.CreatedAt
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
                Log.Error("GetTargetNodeByKey: no available nodes for key {Key}", key);
                throw new RpcException(new Status(StatusCode.Unavailable, "No available hashing nodes to route the request"));
            }
        }
    }
}
