using System.Xml;
using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

[Obsolete]
public class RestartCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override Dictionary<string, string>? KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; }
    
    /// <inheritdoc />
    public override string Description { get; }
    
    /// <inheritdoc />
    public override string[] AllowedKeys { get; } = [];
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; } = [];
    
    /// <inheritdoc />
    public override bool ValueIsRequired { get; }
    
    /// <inheritdoc />
    public override string ExpectedValue { get; }
    
    /// <inheritdoc />
    public override string[] Examples { get; } = [];
    
    /// <inheritdoc />
    public override Type CommandType { get; } = typeof(RestartCommand);

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}