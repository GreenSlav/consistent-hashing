using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда отключения от диспетчера
/// </summary>
public class DisconnectCommand : CommandBase
{
    /// <inheritdoc />
    public override string Name { get; } = "disconnect";

    /// <inheritdoc />
    public override string Description { get; } = "Отключение от конкретного диспетчера";
    
    /// <inheritdoc />
    public override CommandKey[] AllowedKeys { get; }
    
    /// <inheritdoc />
    public override CommandKey[] RequiredKeys { get; }

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;
    
    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;
    
    /// <inheritdoc />
    public override string[] Examples { get; }
}