namespace Core.Exceptions;

public class KeyNotAllowedException : ParserLogicException
{
    public KeyNotAllowedException(string message) : base(message)
    {
    }
}