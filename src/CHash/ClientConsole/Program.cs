// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;

using var channel = GrpcChannel.ForAddress("https://localhost:8080");
var client = new ProductService.ProductServiceClient(channel);
Console.WriteLine("Creation:");
var createdProduct = await client.CreateProductAsync(new CreateProductRequest
{
    Name = "Buzz",
    Price = 421,
    StockQuantity = 690,
});

Console.WriteLine(createdProduct.Id);
Console.WriteLine("\n\nGetting: ");

var savedProduct = await client.GetProductAsync(new ProductIdRequest()
{
    Id = createdProduct.Id
});

Console.WriteLine(savedProduct.Name);
Console.WriteLine(savedProduct.Price);
Console.WriteLine(savedProduct.StockQuantity);
Console.WriteLine("\n\nUpdating: ");


var updatedProduct = await client.UpdateProductAsync(new UpdateProductRequest
{
    Id            = savedProduct.Id,
    Name          = savedProduct.Name,
    Price         = 9999999,
    StockQuantity = savedProduct.StockQuantity
});

Console.WriteLine(updatedProduct.Id);
Console.WriteLine(updatedProduct.Name);
Console.WriteLine(updatedProduct.Price);
Console.WriteLine(updatedProduct.StockQuantity);

Console.WriteLine("\n\nDeleteing: ");

var deletedProduct = await client.DeleteProductAsync(new ProductIdRequest()
{
    Id = updatedProduct.Id
});

Console.WriteLine("\n\nGetting after deletion: ");

var getAfterDelete = await client.GetProductAsync(new ProductIdRequest()
{
    Id = updatedProduct.Id
});

Console.WriteLine(getAfterDelete.Name);
Console.WriteLine(getAfterDelete.Price);
Console.WriteLine(getAfterDelete.StockQuantity);