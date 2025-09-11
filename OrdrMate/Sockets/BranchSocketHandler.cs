using System.Text.Json;
using OrdrMate.Features.BranchAttendance;

namespace OrdrMate.Sockets;

public class BranchSocketHandler : BaseSocketHandler
{
    public BranchSocketHandler()
    {
        AttendanceCodeEvent.CodeRegenerated += SendAuthCode;
    }

    public void SendAuthCode(string branchId, string code)
    {
        var message = new
        {
            type = "AttendanceCodeUpdate",
            code
        };

        _ = SendTo(branchId, JsonSerializer.Serialize(message));
    }
}