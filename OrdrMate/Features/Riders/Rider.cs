using OrdrMate.Models;

namespace OrdrMate.Features.Riders;

public class RiderProfile
{
    public required string UserId { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public User? User { get; set; }
}