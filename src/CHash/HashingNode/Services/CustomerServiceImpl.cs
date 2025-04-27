using Grpc.Core;
using ProtosInterfaceDispatcher.Protos;
using System.Collections.Concurrent;
using Google.Protobuf.WellKnownTypes;

namespace HashingNode.Services;

/// <summary>
/// Реализация CRUD операций для CustomerService
/// </summary>
public class CustomerServiceImpl : ProtosInterfaceDispatcher.Protos.CustomerService.CustomerServiceBase
{
    private static readonly ConcurrentDictionary<string, CustomerDto> _customers = new();
    private readonly ILogger<CustomerServiceImpl> _logger;

    public CustomerServiceImpl(ILogger<CustomerServiceImpl> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public override Task<CustomerDto> CreateCustomer(CreateCustomerRequest request, ServerCallContext context)
    {
        var customer = new CustomerDto
        {
            Id = request.Id,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow.ToString("O") // ISO 8601 format
        };

        _customers[customer.Id] = customer;
        _logger.LogInformation("Created customer {CustomerId}", customer.Id);
        
        return Task.FromResult(customer);
    }

    /// <inheritdoc/>
    public override Task<CustomerDto> GetCustomer(CustomerIdRequest request, ServerCallContext context)
    {
        if (_customers.TryGetValue(request.Id, out var customer))
        {
            return Task.FromResult(customer);
        }

        _logger.LogWarning("Customer {CustomerId} not found", request.Id);
        throw new RpcException(
            new Status(StatusCode.NotFound, $"Customer {request.Id} not found"));
    }

    /// <inheritdoc/>
    public override Task<CustomerDto> UpdateCustomer(UpdateCustomerRequest request, ServerCallContext context)
    {
        if (!_customers.ContainsKey(request.Id))
        {
            _logger.LogWarning("Update failed: customer {CustomerId} not found", request.Id);
            throw new RpcException(
                new Status(StatusCode.NotFound, $"Customer {request.Id} not found"));
        }

        var updatedCustomer = new CustomerDto
        {
            Id = request.Id,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = _customers[request.Id].CreatedAt // Сохраняем оригинальную дату создания
        };

        _customers[request.Id] = updatedCustomer;
        _logger.LogInformation("Updated customer {CustomerId}", request.Id);
        
        return Task.FromResult(updatedCustomer);
    }

    /// <inheritdoc/>
    public override Task<DeleteCustomerResponse> DeleteCustomer(CustomerIdRequest request, ServerCallContext context)
    {
        if (_customers.TryRemove(request.Id, out _))
        {
            _logger.LogInformation("Deleted customer {CustomerId}", request.Id);
            return Task.FromResult(new DeleteCustomerResponse { Success = true });
        }

        _logger.LogWarning("Delete failed: customer {CustomerId} not found", request.Id);
        throw new RpcException(
            new Status(StatusCode.NotFound, $"Customer {request.Id} not found"));
    }

    /// <inheritdoc/>
    public override Task<CustomerList> ListCustomers(Empty request, ServerCallContext context)
    {
        var response = new CustomerList();
        response.Customers.AddRange(_customers.Values);
        _logger.LogInformation("Returned {Count} customers", response.Customers.Count);
        return Task.FromResult(response);
    }
}