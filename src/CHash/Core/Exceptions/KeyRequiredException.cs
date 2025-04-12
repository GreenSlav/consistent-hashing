namespace Core.Exceptions;

public class KeyRequiredException : ParserLogicException
{
    public KeyRequiredException(string message) : base(message)
    {
    }
}