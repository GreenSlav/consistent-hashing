namespace Core.Exceptions;

public class KeyValueRequiredException : ParserLogicException
{
    public KeyValueRequiredException(string message) : base(message)
    {
    }
}