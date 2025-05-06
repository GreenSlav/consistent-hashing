using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Internal = ProtosInterfaceDispatcher.Protos.Internal;

namespace HashingNode.Services
{
    /// <summary>
    /// Реализация gRPC-сервиса для работы с клиентами на hashing-ноде.
    /// Использует «Internal» контракты.
    /// </summary>
    public class CustomerServiceImpl : Internal.CustomerService.CustomerServiceBase
    {
        // Thread-safe хранилище Internal.CustomerDto
        private static readonly ConcurrentDictionary<string, Internal.CustomerDto> _customers
            = new();

        private readonly ILogger<CustomerServiceImpl> _logger;

        public CustomerServiceImpl(ILogger<CustomerServiceImpl> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override Task<Internal.CustomerDto> CreateCustomer(
            Internal.CreateCustomerRequest request,
            ServerCallContext context)
        {
            var dto = new Internal.CustomerDto
            {
                Id          = request.Id,
                FullName    = request.FullName,
                Email       = request.Email,
                PhoneNumber = request.PhoneNumber,
                CreatedAt   = DateTime.UtcNow.ToString("O")
            };

            _customers[dto.Id] = dto;
            _logger.LogInformation("Created internal customer {CustomerId}", dto.Id);
            return Task.FromResult(dto);
        }

        /// <inheritdoc/>
        public override Task<Internal.CustomerDto> GetCustomer(
            Internal.CustomerIdRequest request,
            ServerCallContext context)
        {
            if (_customers.TryGetValue(request.Id, out var dto))
            {
                return Task.FromResult(dto);
            }

            _logger.LogWarning("Internal customer {CustomerId} not found", request.Id);
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Customer {request.Id} not found"));
        }

        /// <inheritdoc/>
        public override Task<Internal.CustomerDto> UpdateCustomer(
            Internal.UpdateCustomerRequest request,
            ServerCallContext context)
        {
            if (!_customers.TryGetValue(request.Id, out var existing))
            {
                _logger.LogWarning("Update failed: customer {CustomerId} not found", request.Id);
                throw new RpcException(new Status(
                    StatusCode.NotFound,
                    $"Customer {request.Id} not found"));
            }

            var updated = new Internal.CustomerDto
            {
                Id          = request.Id,
                FullName    = request.FullName,
                Email       = request.Email,
                PhoneNumber = request.PhoneNumber,
                CreatedAt   = existing.CreatedAt // сохраняем дату создания
            };

            _customers[request.Id] = updated;
            _logger.LogInformation("Updated internal customer {CustomerId}", request.Id);
            return Task.FromResult(updated);
        }

        /// <inheritdoc/>
        public override Task<Internal.DeleteCustomerResponse> DeleteCustomer(
            Internal.CustomerIdRequest request,
            ServerCallContext context)
        {
            if (_customers.TryRemove(request.Id, out _))
            {
                _logger.LogInformation("Deleted internal customer {CustomerId}", request.Id);
                return Task.FromResult(new Internal.DeleteCustomerResponse { Success = true });
            }

            _logger.LogWarning("Delete failed: customer {CustomerId} not found", request.Id);
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Customer {request.Id} not found"));
        }

        /// <inheritdoc/>
        public override Task<Internal.CustomerList> ListCustomers(
            Empty request,
            ServerCallContext context)
        {
            var list = new Internal.CustomerList();
            list.Customers.AddRange(_customers.Values);
            _logger.LogInformation("Listed {Count} internal customers", list.Customers.Count);
            return Task.FromResult(list);
        }
    }
}