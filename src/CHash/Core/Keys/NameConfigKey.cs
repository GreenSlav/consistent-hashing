using Core.Enums;

namespace Core.Keys;

public class NameConfigKey: ConfigKey
{
    public NameConfigKey()
    {
        ShortName = "n";
        KeyName = CommandKey.Name;
        ValueIsRequired = true;
        ExpectedValue = "string";
    }
}