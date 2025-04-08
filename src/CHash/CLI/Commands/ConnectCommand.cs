using CLI.Abstractions;

namespace CLI.Commands;

/// <summary>
/// Команда подключения к диспетчеру
/// </summary>
public class ConnectCommand : CommandBase
{
    /// <inheritdoc />
    public override string? Value { get; set; }
    
    /// <inheritdoc />
    public override Dictionary<string, string>? KeyAndValues { get; set; }
    
    /// <inheritdoc />
    public override string Name { get; } = "connect";

    /// <inheritdoc />
    public override string Description { get; } = "На постоянной основе подключиться к определенному диспетчеру";
    
    /// <inheritdoc />
    public override string[] AllowedKeys { get; } = [];
    
    /// <inheritdoc />
    public override string[] RequiredKeys { get; } = [];

    /// <inheritdoc />
    public override bool ValueIsRequired { get; } = true;

    /// <inheritdoc />
    public override string ExpectedValue { get; } = "Идентификатор диспетчера, к которому подключаемся";

    /// <inheritdoc />
    public override string[] Examples { get; } = [];

    /// <inheritdoc />
    public override Type CommandType { get; } = typeof(ConnectCommand);


    /// <inheritdoc />
    public override Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}