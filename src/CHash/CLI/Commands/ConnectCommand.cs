using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда подключения к диспетчеру
/// </summary>
public class ConnectCommand : CommandBase
{
    public ConnectCommand(Dictionary<string, string> dict) : base(dict)
    {}
    
    /// <inheritdoc />
    public override string Name { get; } = "connect";

    /// <inheritdoc />
    public override string Description { get; } = "На постоянной основе подключиться к определенному диспетчеру";
    
    /// <inheritdoc />
    public override string[] AllowedKeys { get; }
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; }

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера, к которому подключаемся";

    /// <inheritdoc />
    public override string Value { get; }

    /// <inheritdoc />
    public override string[] Examples { get; }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}