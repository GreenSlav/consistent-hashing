namespace Core.Abstractions;

/// <summary>
/// Базовый класс команды
/// </summary>
public abstract class CommandBase
{
    /// <summary>
    /// Переданное значение
    /// </summary>
    public abstract string? Value { get; set; }
    
    /// <summary>
    /// Словарь ключей к команде и их значений
    /// </summary>
    public abstract Dictionary<string, string?>? KeyAndValues { get; set; }
    
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
    public abstract KeyBase[] AllowedKeys { get; }
    
    /// <summary>
    /// Обязательные ключи
    /// </summary>
    public abstract KeyBase[] RequiredKeys { get; }
    
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
    
    
    /// <summary>
    /// Тип команды
    /// </summary>
    public abstract Type CommandType { get; }
    
    /// <summary>
    /// Что необходимо выполнить в ответ на команду
    /// </summary>
    /// <returns></returns>
    public abstract Task ExecuteAsync();

    /// <summary>
    /// Вывод примеров использования
    /// </summary>
    public virtual void PrintUsage()
    {
        foreach (var example in Examples)
        {
            Console.WriteLine(example);
        }
    }
}