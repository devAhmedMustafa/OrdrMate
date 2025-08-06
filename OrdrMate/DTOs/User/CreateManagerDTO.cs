using System.ComponentModel.DataAnnotations;

namespace OrdrMate.DTOs.User;

public class CreateManagerDTO
{
    [Required, MinLength(3)] public required string Username { get; set; }
    [Required, MinLength(8)] public required string Password { get; set; }
    public string? Role { get; set; }
}