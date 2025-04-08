using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Вывод работающих диспетчеров
/// </summary>
public class ListCommand : CommandBase
{
    /// <inheritdoc />
    public override string Name { get; } = "list";

    /// <inheritdoc />
    public override string Description { get; } = "Список запущенных диспетчеров";
    
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