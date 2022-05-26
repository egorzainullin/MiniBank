namespace MiniBank.Core.Exceptions;

public class ValidationException: System.Exception
{
    public ValidationException(string message)
    : base(message) { }
}