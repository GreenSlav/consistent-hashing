using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда остановки диспетчера
/// </summary>
public class KillCommand : CommandBase
{
    public KillCommand(Dictionary<string, string> dict) : base(dict)
    {}
    
    /// <inheritdoc />
    public override string Name { get; } = "kill";

    /// <inheritdoc />
    public override string Description { get; } = "Остановить экземпляр диспетчера";
    
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