namespace OrdrMate.Features.UserPassword;

public class ChangePasswordDto
{
    public required string UserId { get; set; }
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}
