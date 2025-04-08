using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

[Obsolete]
public class RestartCommand : CommandBase
{
    /// <inheritdoc />
    public override string Name { get; }
    
    /// <inheritdoc />
    public override string Description { get; }
    
    /// <inheritdoc />
    public override string[] AllowedKeys { get; }
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; }
    
    /// <inheritdoc />
    public override bool ValueIsRequired { get; }
    
    /// <inheritdoc />
    public override string ExpectedValue { get; }
    
    /// <inheritdoc />
    public override string[] Examples { get; }
}