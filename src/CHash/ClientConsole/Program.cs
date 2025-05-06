// ClientConsoleForOrderService.cs
using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos.External;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== OrderService Demo ===");

        // 1) Создаём канал к диспетчеру
        using var channel = GrpcChannel.ForAddress("https://localhost:8080");
        var client = new OrderService.OrderServiceClient(channel);

        // 2) Создаём новый заказ
        Console.WriteLine("\n-- CreateOrder --");
        var createResp = await client.CreateOrderAsync(new CreateOrderRequest
        {
            CustomerId  = "customer123",
            OrderDate   = DateTime.UtcNow.ToString("O"),
            TotalAmount = 123.45
        });
        Console.WriteLine($"Created Order ID: {createResp.Id}");

        // 3) Читаем только что созданный заказ
        Console.WriteLine("\n-- GetOrder --");
        var getResp = await client.GetOrderAsync(new OrderIdRequest
        {
            Id = createResp.Id
        });
        Console.WriteLine($"CustomerId : {getResp.CustomerId}");
        Console.WriteLine($"OrderDate  : {getResp.OrderDate}");
        Console.WriteLine($"TotalAmount: {getResp.TotalAmount}");

        // 4) Обновляем сумму заказа
        Console.WriteLine("\n-- UpdateOrder --");
        var updateResp = await client.UpdateOrderAsync(new UpdateOrderRequest
        {
            Id          = createResp.Id,
            CustomerId  = getResp.CustomerId,
            OrderDate   = getResp.OrderDate,
            TotalAmount = getResp.TotalAmount + 50 // пример: прибавляем 50
        });
        Console.WriteLine($"Updated TotalAmount: {updateResp.TotalAmount}");

        // 5) Удаляем заказ
        Console.WriteLine("\n-- DeleteOrder --");
        var deleteResp = await client.DeleteOrderAsync(new OrderIdRequest
        {
            Id = createResp.Id
        });
        Console.WriteLine($"Delete success: {deleteResp.Success}");

        // 6) Пытаемся снова получить удалённый заказ
        Console.WriteLine("\n-- GetOrder After Deletion --");
        try
        {
            var afterDel = await client.GetOrderAsync(new OrderIdRequest
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
}