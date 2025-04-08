namespace CLI.Abstractions;

public interface IParser
{
    CommandBase? Parse(string[] args);
}