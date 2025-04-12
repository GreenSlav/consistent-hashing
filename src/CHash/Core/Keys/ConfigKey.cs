using Core.Abstractions;
using Core.Enums;

namespace Core.Keys;

public class ConfigKey: KeyBase
{
    public ConfigKey()
    {
        ShortName = "c";
        KeyName = CommandKey.Config;
        ValueIsRequired = false;
        ExpectedValue = "path";
    }

}