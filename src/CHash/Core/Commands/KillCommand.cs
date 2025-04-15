using Core.Abstractions;
using Core.Enums;
using Core.Keys;
using Grpc.Net.Client;
using ProtosInterfaceDispatcher.Protos;

namespace Core.Commands;

/// <summary>
/// Команда остановки диспетчера
/// </summary>
public class KillCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }

    /// <inheritdoc />
    public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }

    /// <inheritdoc />
    public override string Name { get; } = "kill";

    /// <inheritdoc />
    public override string Description { get; } = "Остановить экземпляр диспетчера";

    /// <inheritdoc />
    public override KeyBase[] AllowedKeys { get; } =
    {
        new DispatcherPortKey(),
    };

    /// <inheritdoc />
    public override KeyBase[] RequiredKeys { get; } =
    {
        new DispatcherPortKey(),
    };

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера";

    /// <inheritdoc />
    public override string[] Examples { get; } =
    {
        "kill -p 8080"
    };

    public override Type CommandType { get; } = typeof(KillCommand);

    public override async Task ExecuteAsync()
    {
        // Проверяем, что коллекция ключей не null
        if (KeyAndValues == null)
            throw new Exception("Не переданы параметры для команды.");

        // Извлекаем значение порта из коллекции (используем ключ DispatcherPortKey)
        var portEntry = KeyAndValues
            .FirstOrDefault(x => x.Key.KeyName == CommandKey.Port);
        string? portStr = portEntry.Value;
        if (string.IsNullOrEmpty(portStr))
            throw new Exception("Не указан порт для диспетчера.");

        // Преобразуем порт в число
        if (!int.TryParse(portStr, out int port))
            throw new Exception("Порт должен быть числовым значением.");

        // TODO: Исправить потом, сделать так чтобы по https все было
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        
        // Формируем адрес для gRPC соединения
        string address = $"https://localhost:{port}";
        try
        {
            // Создаём gRPC канал на основе адреса диспетчера
            using var channel = GrpcChannel.ForAddress(address);

            // Создаем клиента для сервиса Dispatcher
            var client = new Dispatcher.DispatcherClient(channel);

            // Формируем запрос на Shutdown.
            // В данном случае, если Value содержит идентификатор диспетчера, передаем его,
            // иначе отправляем пустую строку.
            var shutdownRequest = new ShutdownRequest
            {
                DispatcherId = Value ?? string.Empty
            };

            // Вызываем метод Shutdown
            var response = await client.ShutdownAsync(shutdownRequest);

            Console.WriteLine($"Shutdown Response: Success = {response.Success}, Message = {response.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при вызове Shutdown: {ex.Message}");
            throw;
        }

        await Task.CompletedTask;
    }
}