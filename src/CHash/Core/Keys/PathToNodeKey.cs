using Core.Abstractions;
using Core.Enums;

namespace Core.Keys;

public class PathToNodeKey : KeyBase
{
    /// <inheritdoc />
    public override string ShortName { get; set; } = "t";

    /// <inheritdoc />
    public override string KeyName { get; set; } = CommandKey.PathToNode;
    
    /// <inheritdoc />
    public override bool ValueIsRequired { get; set; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; set; } = "Путь к исполняемому файлу диспетчера";
}