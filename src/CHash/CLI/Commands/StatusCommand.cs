using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда проверки статуса диспетчера
/// </summary>
public class StatusCommand : CommandBase
{
    public StatusCommand(Dictionary<string, string> dict) : base(dict)
    {}
    
    /// <inheritdoc />
    public override string Name { get; } = "status";

    /// <inheritdoc />
    public override string Description { get; } = "Показывает статус конкретного диспетчера";
    
    /// <inheritdoc />
    public override string[] AllowedKeys { get; }
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; }

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера";
    
    /// <inheritdoc />
    public override string Value { get; }
    
    /// <inheritdoc />
    public override string[] Examples { get; }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}