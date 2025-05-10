using Microsoft.Extensions.DependencyInjection;
using Serilog;
using HashingNode.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog через конфигурацию в appsettings.json (опционально)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/hashing_node_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Использование Serilog как провайдера логгирования в ASP.NET Core
builder.Host.UseSerilog(); 

// Добавление gRPC-сервисов
builder.Services.AddGrpc();
// Можно добавить другие зависимости, если используются

var app = builder.Build();

// Регистрация gRPC-сервисов
app.MapGrpcService<ProductServiceImpl>();
app.MapGrpcService<OrderServiceImpl>();
app.MapGrpcService<CustomerServiceImpl>();

// Тестовая страница
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();