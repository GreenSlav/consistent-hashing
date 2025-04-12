namespace Core.Exceptions;

public class NonExistingKeyException : ParserLogicException
{
    public NonExistingKeyException(string message) : base(message)
    {
    }
}