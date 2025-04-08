using CLI.Enums;

namespace CLI.Abstractions;

/// <summary>
/// Базовый класс команды
/// </summary>
public abstract class CommandBase
{
    /// <summary>
    /// Название команды
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// Описание команды
    /// </summary>
    public abstract string Description { get; }
    
    /// <summary>
    /// Разрешенные ключи
    /// </summary>
    public abstract string[] AllowedKeys { get; }
    
    /// <summary>
    /// Обязательные ключи
    /// </summary>
    public abstract string[] RequiredKeys { get; }
    
    /// <summary>
    /// Обязательное ли значение
    /// </summary>
    public abstract bool ValueIsRequired { get; }
    
    /// <summary>
    /// Что ожидается на вход (если вход требуется: ValueIsRequired == true)
    /// </summary>
    public abstract string ExpectedValue { get; }
    
    /// <summary>
    /// Примеры использования
    /// </summary>
    public abstract string[] Examples { get; }
}