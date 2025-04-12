using Core.Abstractions;
using Core.Keys;

namespace Core.Commands;

[Obsolete]
public class RestartCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }

    /// <inheritdoc />
    public override Dictionary<string, string?>? KeyAndValues { get; set; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override string Description { get; }

    /// <inheritdoc />
    public override KeyBase[] AllowedKeys { get; } =
    {
        new ConfigKey(),
        new NameConfigKey(),
        new PortConfigKey(),
    };


    /// <inheritdoc />
    public override KeyBase[] RequiredKeys { get; } =
    {
        new NameConfigKey(),
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