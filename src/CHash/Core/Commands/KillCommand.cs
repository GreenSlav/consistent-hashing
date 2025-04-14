using Core.Abstractions;
using Core.Keys;

namespace Core.Commands;

/// <summary>
/// Команда остановки диспетчера
/// </summary>
public class KillCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "kill";

    /// <inheritdoc />
    public override string Description { get; } = "Остановить экземпляр диспетчера";

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
        "kill -n dispatcher1",
        "kill -n dispatcher2 -p 8080"
    };

    public override Type CommandType { get; } = typeof(KillCommand);

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}