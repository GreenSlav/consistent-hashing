using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Keys;

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