namespace OrdrMate.Models;

using OrdrMate.Enums;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Username { get; set; }
    public required string Password { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
}