using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда отключения от диспетчера
/// </summary>
public class DisconnectCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override Dictionary<string, string>? KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "disconnect";

    /// <inheritdoc />
    public override string Description { get; } = "Отключение от конкретного диспетчера";

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
    public override bool ValueIsRequired { get; } = false;
    
    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;
    
    /// <inheritdoc />
    public override string Value { get; }
    public override string[] Examples { get; } = [];
    
    /// <inheritdoc />
    public override Type CommandType { get; } = typeof(DisconnectCommand);

    /// <inheritdoc />
    public override string[] Examples { get; } =
    {
        "disconnect -n dispatcher1",
        "disconnect -n dispatcher2 -p 8080"
    };
    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}