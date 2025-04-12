using Core.Abstractions;
using Core.Keys;

namespace Core.Commands;

/// <summary>
/// Команда отключения от диспетчера
/// </summary>
public class DisconnectCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override Dictionary<string, string?>? KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "disconnect";

    /// <inheritdoc />
    public override string Description { get; } = "Отключение от конкретного диспетчера";

    /// <inheritdoc />
    public override KeyBase[] AllowedKeys { get; } =
    {
        new NameConfigKey(),
        new PortConfigKey(),
    };

    /// <inheritdoc />
    public override KeyBase[] RequiredKeys { get; } =
    {
        new NameConfigKey(),
    };

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;
    
    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;
    
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