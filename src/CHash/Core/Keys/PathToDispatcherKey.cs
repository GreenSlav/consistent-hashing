using Core.Abstractions;
using Core.Enums;

namespace Core.Keys;

public class PathToDispatcherKey : KeyBase
{
    /// <inheritdoc />
    public override string ShortName { get; set; } = "d";

    /// <inheritdoc />
    public override string KeyName { get; set; } = CommandKey.PathToDispatcher;
    
    /// <inheritdoc />
    public override bool ValueIsRequired { get; set; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; set; } = "Путь к исполняемому файлу диспетчера";
}