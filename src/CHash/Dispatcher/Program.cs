using Dispatcher.Helpers;
using Dispatcher.Services;
using Serilog;

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Добавляем gRPC-сервисы
builder.Services.AddGrpc();

// Регистрация NodeRegistry как Singleton
builder.Services.AddSingleton<NodeRegistry>();

// Регистрация сервисов с логированием через Serilog
builder.Host.UseSerilog(); 

var app = builder.Build();

// Регистрация gRPC-сервисов
app.MapGrpcService<DispatcherService>();
app.MapGrpcService<ProductService>();
app.MapGrpcService<OrderService>();
app.MapGrpcService<CustomerService>();

// Тестовая страница
app.MapGet("/", () => "gRPC endpoints доступны через специализированный клиент. Подробнее: https://go.microsoft.com/fwlink/?linkid=2086909 ");

app.Run();