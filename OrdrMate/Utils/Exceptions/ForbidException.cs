namespace OrdrMate.Utils.Exceptions;

public class ForbidException : OException
{
    public ForbidException(string message) : base(message, 403) { }
}