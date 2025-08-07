namespace OrdrMate.Utils.Exceptions;

public class OException : Exception
{
    public string Message { get; }
    public int StatusCode { get; }

    public OException(string message, int statusCode = 500)
    {
        Message = message;
        StatusCode = statusCode;
    }
}