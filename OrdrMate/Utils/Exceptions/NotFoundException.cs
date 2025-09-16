namespace OrdrMate.Utils.Exceptions;

public class NotFoundException : OException
{
    public NotFoundException(string message) : base(message, 404){}
}