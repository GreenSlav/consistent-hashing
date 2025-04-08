using CLI.Enums;

namespace CLI.Abstractions;

public abstract class CommandBase
{
    public abstract string Name { get; }
    
    public abstract string Description { get; }
    
    public abstract CommandKey[] AllowedKeys { get; }
}