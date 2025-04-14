using Core.Abstractions;
using Core.Enums;

namespace Core.Keys;

public class BackgroundKey : KeyBase
{
    /// <inheritdoc />
    public override string ShortName { get; set; } = "b";

    /// <inheritdoc />
    public override string KeyName { get; set; } = CommandKey.Background;
    
    /// <inheritdoc />
    public override bool ValueIsRequired { get; set; } = false;

    /// <inheritdoc />
    public override string ExpectedValue { get; set; } = "Нужно ли выполнять задачу в бэкграунде";
}