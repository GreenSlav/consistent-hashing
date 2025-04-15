using Core.Abstractions;
using Core.Enums;
using Core.Keys;
using ProtosInterfaceDispatcher.Protos;

namespace Core.Commands;

/// <summary>
/// Команда добавления узла
/// </summary>
public class AddNodeCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }

    /// <inheritdoc />
    public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }

    /// <inheritdoc />
    public override string Name { get; } = "add";

    /// <inheritdoc />
    public override string Description { get; } = "Создать узел хэширования";

    /// <inheritdoc />
    public override KeyBase[] AllowedKeys { get; } =
    {
        new DispatcherPortKey(),
        new HashingNodePortKey(),
        new PathToNodeKey(),
    };

    /// <inheritdoc />
    public override KeyBase[] RequiredKeys { get; } =
    {
        new DispatcherPortKey(),
        new HashingNodePortKey(),
        new PathToNodeKey(),
    };


    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;

    /// <inheritdoc />
    public override string[] Examples { get; } =
    {
        "add -p 8080 -n 8081",
        "add --port 8080 --node-port 8081"
    };

    public override Type CommandType { get; } = typeof(AddNodeCommand);

    /// <inheritdoc />
    public override async Task ExecuteAsync()
    {
        if (KeyAndValues == null)
            throw new Exception("Не переданы параметры для команды.");

        // Извлекаем порт диспетчера
        var dispatcherPortEntry = KeyAndValues
            .FirstOrDefault(x => x.Key.KeyName == CommandKey.Port);
        string? dispatcherPortStr = dispatcherPortEntry.Value;
        if (string.IsNullOrEmpty(dispatcherPortStr) || !int.TryParse(dispatcherPortStr, out int dispatcherPort))
        {
            throw new Exception("Неверно указан порт диспетчера.");
        }

        // Извлекаем порт для ноды (HashingNodePortKey)
        var nodePortEntry = KeyAndValues
            .FirstOrDefault(x => x.Key.KeyName == CommandKey.NodePort);
        string? nodePortStr = nodePortEntry.Value;
        if (string.IsNullOrEmpty(nodePortStr) || !int.TryParse(nodePortStr, out int nodePort))
        {
            throw new Exception("Неверно указан порт для узла.");
        }

        // Извлекаем путь к ноде (PathToNodeKey)
        var nodePathEntry = KeyAndValues
            .FirstOrDefault(x => x.Key.KeyName == CommandKey.PathToNode);
        string? nodePath = nodePathEntry.Value;
        if (string.IsNullOrEmpty(nodePath))
        {
            throw new Exception("Не указан путь к исполняемому файлу узла.");
        }

        // Настраиваем gRPC канал для подключения к диспетчеру
        // Здесь адрес формируется с учетом HTTPS или HTTP по вашей конфигурации.
        string dispatcherAddress = $"https://localhost:{dispatcherPort}";
        using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(dispatcherAddress);
        var dispatcherClient = new Dispatcher.DispatcherClient(channel);

        // Формируем запрос на создание узла. Расширяем его, передавая preferred_port и node_path.
        var createNodeRequest = new CreateNodeRequest
        {
            PreferredPort = nodePort,
            NodePath = nodePath
        };

        CreateNodeResponse response;
        try
        {
            response = await dispatcherClient.CreateNodeAsync(createNodeRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при вызове CreateNode: {ex.Message}");
            throw;
        }

        if (response.Success)
        {
            Console.WriteLine($"Узел успешно создан. Узел ID: {response.NodeId}, порт: {response.Port}");
        }
        else
        {
            Console.WriteLine($"Не удалось создать узел: {response.Message}");
        }

        await Task.CompletedTask;
    }
}