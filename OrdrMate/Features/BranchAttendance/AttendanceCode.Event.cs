namespace OrdrMate.Features.BranchAttendance;

public class AttendanceCodeEvent
{
    public static event Action<string, string>? CodeRegenerated;

    public static void OnCodeRegenerated(string branchId, string newCode)
    {
        CodeRegenerated?.Invoke(branchId, newCode);
    }
}