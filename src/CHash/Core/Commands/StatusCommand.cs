using Core.Abstractions;
using Core.Keys;

namespace Core.Commands;

/// <summary>
/// Команда проверки статуса диспетчера
/// </summary>
public class StatusCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "status";

    /// <inheritdoc />
    public override string Description { get; } = "Показывает статус конкретного диспетчера";
    
    /// <inheritdoc />
    public override KeyBase[] AllowedKeys { get; } = 
    {
        new DispatcherPortKey(),
    };
    
    /// <inheritdoc />
    public override KeyBase[] RequiredKeys { get; } = 
    {
    };

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера";
    
    /// <inheritdoc />
    public override string[] Examples { get; } = 
    {
        "status -n dispatcher1",
        "status -n dispatcher2 -p 8080"
    };
    public override Type CommandType { get; } = typeof(StatusCommand);

    public override Task ExecuteAsync()
    {
        Console.WriteLine(nameof(StatusCommand));
        return Task.CompletedTask;
    }
}