namespace OrdrMate.Utils.Exceptions;

public class UnauthorizedException : OException
{
    public UnauthorizedException(string message) : base(message, 401) {}
}