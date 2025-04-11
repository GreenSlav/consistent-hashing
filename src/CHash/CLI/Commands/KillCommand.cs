using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда остановки диспетчера
/// </summary>
public class KillCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override Dictionary<string, string>? KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "kill";

    /// <inheritdoc />
    public override string Description { get; } = "Остановить экземпляр диспетчера";

    /// <inheritdoc />
    public override string[] AllowedKeys { get; } =
    {
        CommandKey.Name,
        CommandKey.Port
    };

    /// <inheritdoc />
    public override string[] RequiredKeys { get; } =
    {
        CommandKey.Name
    };

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера";
    
    /// <inheritdoc />
    public override string Value { get; }

    /// <inheritdoc />
    public override string[] Examples { get; } =
    {
        "kill -n dispatcher1",
        "kill -n dispatcher2 -p 8080"
    };

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}