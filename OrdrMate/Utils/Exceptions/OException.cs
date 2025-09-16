namespace OrdrMate.Utils.Exceptions;

public class OException(string message, int statusCode = 500) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}