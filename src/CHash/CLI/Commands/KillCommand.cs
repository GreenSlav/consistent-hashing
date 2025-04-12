using CLI.Abstractions;
using CLI.Enums;
using CLI.Keys;

namespace CLI.Commands;

/// <summary>
/// Команда остановки диспетчера
/// </summary>
public class KillCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override Dictionary<string, string?>? KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "kill";

    /// <inheritdoc />
    public override string Description { get; } = "Остановить экземпляр диспетчера";

    /// <inheritdoc />
    public override KeyBase[] AllowedKeys { get; } =
    {
        new NameConfigKey(),
        new PortConfigKey(),
    };

    /// <inheritdoc />
    public override KeyBase[] RequiredKeys { get; } =
    {
        new NameConfigKey(),
    };

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера";
    
    /// <inheritdoc />
    public override string[] Examples { get; } =
    {
        "kill -n dispatcher1",
        "kill -n dispatcher2 -p 8080"
    };

    public override Type CommandType { get; } = typeof(KillCommand);

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}