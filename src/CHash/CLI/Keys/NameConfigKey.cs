using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Keys;

public class NameConfigKey: ConfigKey
{
    public NameConfigKey()
    {
        ShortName = "-n";
        KeyName = CommandKey.Name;
        ValueIsRequired = true;
        ExpectedValue = "string";
    }
}