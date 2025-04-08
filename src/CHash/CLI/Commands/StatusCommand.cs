using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда проверки статуса диспетчера
/// </summary>
public class StatusCommand : CommandBase
{
    /// <inheritdoc />
    public override string Name { get; } = "status";

    /// <inheritdoc />
    public override string Description { get; } = "Показывает статус конкретного диспетчера";
    
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