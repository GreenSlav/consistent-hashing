using CLI.Enums;

namespace CLI.Abstractions;

public abstract class KeyBase
{
    /// <summary>
    /// /wdawdwadawdadwa
    /// </summary>
    public string ShortName { get; set; }
    
    public CommandKey KeyName;
    
    public bool ValueIsRequired { get; set; }
}