namespace CLI.Exceptions;

public class NonExistingKeyException : ParserLogicException
{
    public NonExistingKeyException(string message) : base(message)
    {
    }
}