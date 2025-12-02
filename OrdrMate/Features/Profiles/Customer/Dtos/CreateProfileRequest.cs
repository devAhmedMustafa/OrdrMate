namespace OrdrMate.Features.Profiles.Customer.Dtos;

public class CreateProfileDto
{
    public required string CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}