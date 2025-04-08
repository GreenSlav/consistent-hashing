using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда запуска диспетчера
/// </summary>
public class StartCommand : CommandBase
{
    /// <inheritdoc />
    public override string Name { get; } = "start";

    /// <inheritdoc />
    public override string Description { get; } = "Запуск нового экземпляра диспетчера";
    
    /// <inheritdoc />
    public override string[] AllowedKeys { get; }
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; }
    
    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;
    
    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;
    
    /// <inheritdoc />
    public override string[] Examples { get; }
}