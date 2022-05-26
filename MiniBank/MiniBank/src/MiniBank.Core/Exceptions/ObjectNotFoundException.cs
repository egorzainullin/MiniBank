namespace MiniBank.Core.Exceptions;

public class ObjectNotFoundException : System.Exception
{
    public ObjectNotFoundException(string message)
        : base(message)
    {
    }
}