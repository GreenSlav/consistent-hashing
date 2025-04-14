using Dispatcher.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем gRPC-сервисы
builder.Services.AddGrpc();

var app = builder.Build();

// Регистрируем gRPC сервис диспетчера (например, GreeterTestService)
app.MapGrpcService<GreeterTestService>();
app.MapGrpcService<DispatcherService>();

app.MapGet("/", () => "gRPC endpoints доступны через специализированный клиент. Подробнее: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();