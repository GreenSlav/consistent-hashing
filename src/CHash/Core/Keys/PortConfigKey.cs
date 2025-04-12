using Core.Enums;

namespace Core.Keys;

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