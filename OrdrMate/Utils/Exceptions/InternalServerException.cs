namespace OrdrMate.Utils.Exceptions;

public class InternalServerException : OException
{
    public InternalServerException(string message) : base(message, 500) {}
}