namespace Core.Abstractions;

public interface IParser
{
    CommandBase? Parse(string[] args);
}