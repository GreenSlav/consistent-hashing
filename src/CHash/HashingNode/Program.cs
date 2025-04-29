
using Dispatcher.Services;
using HashingNode.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<GreeterTestServiceImpl>();
app.MapGrpcService<DispatcherServiceImpl>();
app.MapGrpcService<ProductServiceImpl>();
app.MapGrpcService<OrderServiceImpl>();
app.MapGrpcService<CustomerServiceImpl>();

// Configure the HTTP request pipeline.
//app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();