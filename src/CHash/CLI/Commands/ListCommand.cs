using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Вывод работающих диспетчеров
/// </summary>
public class ListCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override Dictionary<string, string>? KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "list";

    /// <inheritdoc />
    public override string Description { get; } = "Список запущенных диспетчеров";

    /// <inheritdoc />
    public override string[] AllowedKeys { get; } =
    {
        CommandKey.Port,
        CommandKey.Name

    };
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; } = [];

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;
    

    /// <inheritdoc />
    public override string[] Examples { get; } =
    {
        "list",
        "list -n dispatcher1",
        "list -p 8080"
    };
    
    /// <inheritdoc />
    public override Type CommandType { get; } = typeof(ListCommand);

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}