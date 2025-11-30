using OrdrMate.Models;

namespace OrdrMate.Features.Profiles.Customer;
public class CustomerProfile
{
    public required int CustomerId { get; set; }
    public required string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public User? User { get; set; }
}