using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Keys;

public class PortConfigKey : ConfigKey
{
    public PortConfigKey()
    {
        ShortName = "p";
        KeyName = CommandKey.Port;
        ValueIsRequired = true;
        ExpectedValue = "number";
    }
}