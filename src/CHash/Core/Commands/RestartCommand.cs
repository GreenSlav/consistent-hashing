using Core.Abstractions;
using Core.Keys;

namespace Core.Commands;

[Obsolete]
public class RestartCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }

    /// <inheritdoc />
    public override required IEnumerable<KeyValuePair<KeyBase, string?>> KeyAndValues { get; set; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override string Description { get; }

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
    public override bool ValueIsRequired { get; }

    /// <inheritdoc />
    public override string ExpectedValue { get; }

    /// <inheritdoc />
    public override string[] Examples { get; } =
    {
        "restart -n dispatcher1",
        "restart -n dispatcher2 -p 8080 -c config.json"
    };

    public override Type CommandType { get; } = typeof(RestartCommand);

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}