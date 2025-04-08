using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

[Obsolete]
public class RestartCommand : CommandBase
{
    public RestartCommand(Dictionary<string, string> dict) : base(dict)
    {}
    
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
    public override string Value { get; }
    
    /// <inheritdoc />
    public override string[] Examples { get; }

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}