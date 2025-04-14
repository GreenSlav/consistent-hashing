namespace Core.Abstractions;

public abstract class KeyBase
{
    /// <summary>
    /// Короткая запись ключа длиной в один символ (пойдет после одного знака "-")
    /// </summary>
    public abstract string ShortName { get; set; }
    
    /// <summary>
    /// Полная запись ключа (пойдет после двух знаков "--")
    /// </summary>
    public abstract string KeyName { get; set; }
    
    /// <summary>
    /// Требуется ли значение ключу
    /// </summary>
    public abstract bool ValueIsRequired { get; set; }
    
    /// <summary>
    /// Объяснение того, что ожидается на вход
    /// </summary>
    public abstract string ExpectedValue { get; set; }
}