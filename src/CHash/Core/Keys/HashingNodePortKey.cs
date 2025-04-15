using Core.Abstractions;
using Core.Enums;

namespace Core.Keys;

public class HashingNodePortKey : KeyBase
{
    /// <inheritdoc />
    public override string ShortName { get; set; } = "h";
    
    /// <inheritdoc />
    public override string KeyName { get; set; } = CommandKey.NodePort;

    /// <inheritdoc />
    public override bool ValueIsRequired { get; set; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; set; } = "Порт узла хэширования";
}