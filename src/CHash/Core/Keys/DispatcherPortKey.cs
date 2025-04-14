using Core.Abstractions;
using Core.Enums;

namespace Core.Keys;

public class DispatcherPortKey : KeyBase
{
    /// <inheritdoc />
    public override string ShortName { get; set; } = "p";

    /// <inheritdoc />
    public override string KeyName { get; set; } = CommandKey.Port;
    
    /// <inheritdoc />
    public override bool ValueIsRequired { get; set; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; set; } = "Порт, на котором надо запускать диспетчера";
}