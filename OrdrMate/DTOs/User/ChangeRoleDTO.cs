using OrdrMate.Enums;

namespace OrdrMate.DTOs.User;

public class ChangeRoleDTO
{
    public required string UserId { get; set; }
    public required UserRole NewRole { get; set; }
}
