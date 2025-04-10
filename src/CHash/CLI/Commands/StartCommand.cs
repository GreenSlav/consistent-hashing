using CLI.Abstractions;
using CLI.Enums;

namespace CLI.Commands;

/// <summary>
/// Команда запуска диспетчера
/// </summary>
public class StartCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override Dictionary<string, string>? KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "start";

    /// <inheritdoc />
    public override string Description { get; } = "Запуск нового экземпляра диспетчера";

    /// <inheritdoc />
    public override string[] AllowedKeys { get; } = 
    {
        CommandKey.Port,
        CommandKey.Name,
        CommandKey.Config
    };
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; } = 
    {
        CommandKey.Port,
        CommandKey.Name
    };
    
    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = false;
    
    /// <inheritdoc />
    public override string ExpectedValue { get; } = string.Empty;
    
    /// <inheritdoc />
    public override string[] Examples { get; } = 
    {
        "start -n dispatcher1 -p 8080",
        "start -n dispatcher2 -p 9090 -c config.json"
    };
    public override Type CommandType { get; } = typeof(StartCommand);

    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}