using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;
using Dispatcher.Helpers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Dispatcher.Services;

public class CustomerService : ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceBase
{
    private readonly NodeRegistry _nodeRegistry;

    public CustomerService(NodeRegistry nodeRegistry)
    {
        _nodeRegistry = nodeRegistry;
    }

    public override async Task<CustomerDto> GetCustomer(CustomerIdRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);

        return await client.GetCustomerAsync(request);
    }

    public override async Task<CustomerDto> CreateCustomer(CreateCustomerRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.FullName);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);

        return await client.CreateCustomerAsync(request);
    }

    public override async Task<CustomerDto> UpdateCustomer(UpdateCustomerRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);

        return await client.UpdateCustomerAsync(request);
    }

    public override async Task<DeleteCustomerResponse> DeleteCustomer(CustomerIdRequest request, ServerCallContext context)
    {
        var node = GetTargetNode(request.Id);
        using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
        var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);

        return await client.DeleteCustomerAsync(request);
    }

    public override async Task<CustomerList> ListCustomers(Empty request, ServerCallContext context)
    {
        var tasks = _nodeRegistry.GetAllNodes().Select(async node =>
        {
            using var channel = GrpcChannel.ForAddress($"https://localhost:{node.Port}");
            var client = new ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceClient(channel);
            return await client.ListCustomersAsync(request);
        });

        var results = await Task.WhenAll(tasks);
        var combinedList = new CustomerList();
        foreach (var result in results)
        {
            combinedList.Customers.AddRange(result.Customers);
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