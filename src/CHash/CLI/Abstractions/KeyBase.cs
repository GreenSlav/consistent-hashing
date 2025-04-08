using CLI.Enums;

namespace CLI.Abstractions;

public class KeyBase
{
    public string ShortName { get; set; }
    public string KeyName;
    public bool ValueIsRequired { get; set; }
}