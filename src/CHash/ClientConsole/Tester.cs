using Grpc.Core;
using Grpc.Net.Client;
using External = ProtosInterfaceDispatcher.Protos.External;

namespace ClientConsole;

public static class Tester
{
    public static async Task TestCustomer()
    {
        Console.WriteLine("=== CustomerService Demo ===");

        // 1) Создаём канал к диспетчеру
        using var channel = GrpcChannel.ForAddress("https://localhost:8080");
        var client = new External.CustomerService.CustomerServiceClient(channel);

        // 2) Создаём нового клиента
        Console.WriteLine("\n-- CreateCustomer --");
        var createResp = await client.CreateCustomerAsync(new External.CreateCustomerRequest
        {
            FullName    = "Ivan Petrov",
            Email       = "ivan.petrov@example.com",
            PhoneNumber = "+1234567890"
        });
        Console.WriteLine($"Created Customer ID: {createResp.Id}");

        // 3) Читаем только что созданного клиента
        Console.WriteLine("\n-- GetCustomer --");
        var getResp = await client.GetCustomerAsync(new External.CustomerIdRequest
        {
            Id = createResp.Id
        });
        Console.WriteLine($"FullName   : {getResp.FullName}");
        Console.WriteLine($"Email      : {getResp.Email}");
        Console.WriteLine($"PhoneNumber: {getResp.PhoneNumber}");
        Console.WriteLine($"CreatedAt  : {getResp.CreatedAt}");

        // 4) Обновляем email клиента
        Console.WriteLine("\n-- UpdateCustomer --");
        var updateResp = await client.UpdateCustomerAsync(new External.UpdateCustomerRequest
        {
            Id          = createResp.Id,
            FullName    = getResp.FullName,
            Email       = "ivan.newemail@example.com", // меняем email
            PhoneNumber = getResp.PhoneNumber
        });
        Console.WriteLine($"Updated Email: {updateResp.Email}");

        // 5) Удаляем клиента
        Console.WriteLine("\n-- DeleteCustomer --");
        var deleteResp = await client.DeleteCustomerAsync(new External.CustomerIdRequest
        {
            Id = createResp.Id
        });
        Console.WriteLine($"Delete success: {deleteResp.Success}");

        // 6) Пытаемся снова получить удалённого клиента
        Console.WriteLine("\n-- GetCustomer After Deletion --");
        try
        {
            var afterDel = await client.GetCustomerAsync(new External.CustomerIdRequest
            {
                Id = createResp.Id
            });
            // Если придёт сюда — что-то не так
            Console.WriteLine("ERROR: expected NotFound, got:");
            Console.WriteLine($"  {afterDel.Id} / {afterDel.FullName} / {afterDel.Email}");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            Console.WriteLine($"Customer not found as expected: {ex.Status.Detail}");
        }
    }

    public static async Task TestOrder()
    {
        Console.WriteLine("=== OrderService Demo ===");

        // 1) Создаём канал к диспетчеру
        using var channel = GrpcChannel.ForAddress("https://localhost:8080");
        var client = new External.OrderService.OrderServiceClient(channel);

        // 2) Создаём новый заказ
        Console.WriteLine("\n-- CreateOrder --");
        var createResp = await client.CreateOrderAsync(new External.CreateOrderRequest
        {
            CustomerId  = "customer123",
            OrderDate   = DateTime.UtcNow.ToString("O"),
            TotalAmount = 123.45
        });
        Console.WriteLine($"Created Order ID: {createResp.Id}");

        // 3) Читаем только что созданный заказ
        Console.WriteLine("\n-- GetOrder --");
        var getResp = await client.GetOrderAsync(new External.OrderIdRequest
        {
            Id = createResp.Id
        });
        Console.WriteLine($"CustomerId : {getResp.CustomerId}");
        Console.WriteLine($"OrderDate  : {getResp.OrderDate}");
        Console.WriteLine($"TotalAmount: {getResp.TotalAmount}");

        // 4) Обновляем сумму заказа
        Console.WriteLine("\n-- UpdateOrder --");
        var updateResp = await client.UpdateOrderAsync(new External.UpdateOrderRequest
        {
            Id          = createResp.Id,
            CustomerId  = getResp.CustomerId,
            OrderDate   = getResp.OrderDate,
            TotalAmount = getResp.TotalAmount + 50 // пример: прибавляем 50
        });
        Console.WriteLine($"Updated TotalAmount: {updateResp.TotalAmount}");

        // 5) Удаляем заказ
        Console.WriteLine("\n-- DeleteOrder --");
        var deleteResp = await client.DeleteOrderAsync(new External.OrderIdRequest
        {
            Id = createResp.Id
        });
        Console.WriteLine($"Delete success: {deleteResp.Success}");

        // 6) Пытаемся снова получить удалённый заказ
        Console.WriteLine("\n-- GetOrder After Deletion --");
        try
        {
            var afterDel = await client.GetOrderAsync(new External.OrderIdRequest
            {
                Id = createResp.Id
            });
            // Если придёт сюда — что-то не так
            Console.WriteLine("ERROR: expected NotFound, got:");
            Console.WriteLine($"  {afterDel.Id} / {afterDel.CustomerId} / {afterDel.TotalAmount}");
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            Console.WriteLine($"Order not found as expected: {ex.Status.Detail}");
        }
    }

    public static async Task TestProduct()
    {
        
    }

}