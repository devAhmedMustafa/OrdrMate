namespace OrdrMate.DTOs.Branch;

public class BranchWorkingHoursDto
{
    public TimeSpan StartWorkingHour { get; set; }
    public TimeSpan EndWorkingHour { get; set; }
    public bool[]? WorkingDays { get; set; }
}
