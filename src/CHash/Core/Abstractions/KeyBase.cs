namespace Core.Abstractions;

public abstract class KeyBase
{
    public string ShortName { get; set; }
    
    public string KeyName;
    
    public bool ValueIsRequired { get; set; }
    
    public string ExpectedValue { get; set; }
}