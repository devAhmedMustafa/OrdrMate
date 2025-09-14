namespace OrdrMate.DTOs.User;

public class LoginSuccessDto
{
    public required string Token { get; set; }
    public required string Role { get; set; }
    public required string PharmacyId { get; set; }
    public string? BranchId { get; set; }
}