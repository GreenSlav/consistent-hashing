using Core.Abstractions;
using Core.Keys;

namespace Core.Commands;

/// <summary>
/// Команда запуска диспетчера
/// </summary>
public class StartCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }

    /// <inheritdoc />
    public override Dictionary<string, string?>? KeyAndValues { get; set; }

    /// <inheritdoc />
    public override string Name { get; } = "start";

    /// <inheritdoc />
    public override string Description { get; } = "Запуск нового экземпляра диспетчера";

    /// <inheritdoc />
    public override KeyBase[] AllowedKeys { get; } =
    {
        new ConfigKey(),
        new NameConfigKey(),
        new PortConfigKey(),
    };

    /// <inheritdoc />
    public override KeyBase[] RequiredKeys { get; } =
    {
        new NameConfigKey(),
        new PortConfigKey(),
    };

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;

    /// <inheritdoc />
    public override string[] Examples { get; } =
    {
        "start -n dispatcher1 -p 8080",
        "start -n dispatcher2 -p 9090 -c config.json"
    };

    public override Type CommandType { get; } = typeof(StartCommand);

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}