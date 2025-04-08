using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда отключения от диспетчера
/// </summary>
public class DisconnectCommand : CommandBase
{
    public DisconnectCommand(Dictionary<string, string> dict) : base(dict)
    {}
    
    /// <inheritdoc />
    public override string Name { get; } = "disconnect";

    /// <inheritdoc />
    public override string Description { get; } = "Отключение от конкретного диспетчера";
    
    /// <inheritdoc />
    public override string[] AllowedKeys { get; }
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; }

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;
    
    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;
    
    /// <inheritdoc />
    public override string Value { get; }
    
    /// <inheritdoc />
    public override string[] Examples { get; }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}