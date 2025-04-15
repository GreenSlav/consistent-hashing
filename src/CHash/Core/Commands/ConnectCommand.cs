using Core.Abstractions;
using Core.Keys;

namespace Core.Commands;

/// <summary>
/// Команда подключения к диспетчеру
/// </summary>
public class ConnectCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "connect";

    /// <inheritdoc />
    public override string Description { get; } = "На постоянной основе подключиться к определенному диспетчеру";

    /// <inheritdoc />
    public override KeyBase[] AllowedKeys { get; } =
    {
        new DispatcherPortKey(),
    };
    
    /// <inheritdoc />
    public override KeyBase[] RequiredKeys { get; } = 
    {
        new DispatcherPortKey(),
    };


    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера, к которому подключаемся";
    
    /// <inheritdoc />
    public override string[] Examples { get; } = 
    {
        "connect -n dispatcher1 -p 8080",
        "connect -c config.json -n dispatcher2 -p 9090"
    };

    public override Type CommandType { get; } = typeof(ConnectCommand);


    /// <inheritdoc />
    public override Task ExecuteAsync()
    {
        Console.WriteLine(nameof(ConnectCommand));
        return Task.CompletedTask;
    }
}