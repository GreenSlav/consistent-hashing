using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда остановки диспетчера
/// </summary>
public class KillCommand : CommandBase
{
    /// <inheritdoc />
    public override string Name { get; } = "kill";

    /// <inheritdoc />
    public override string Description { get; } = "Остановить экземпляр диспетчера";
    
    /// <inheritdoc />
    public override CommandKey[] AllowedKeys { get; }
    
    /// <inheritdoc />
    public override CommandKey[] RequiredKeys { get; }

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера";
    
    /// <inheritdoc />
    public override string[] Examples { get; }
}