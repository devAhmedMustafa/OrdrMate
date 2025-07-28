namespace OrdrMate.Utils;

public static class TimeService
{
    private static DateOnly lastCheckTime = DateOnly.FromDateTime(DateTime.Now);

    public static DateOnly GetCurrentDate()
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Now);
        if (currentDate != lastCheckTime)
        {
            lastCheckTime = currentDate;
        }
        return lastCheckTime;
    }

    public static bool IsSameDay(DateOnly date)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Now);
        return date == currentDate;
    }

    public static bool CheckWithinTimeInterval(TimeSpan start, TimeSpan end, bool[] workingDays)
    {
        var currentTime = DateTime.Now.TimeOfDay;
        var currentDay = (int)DateTime.Now.DayOfWeek;

        return currentTime >= start && currentTime <= end && workingDays[currentDay];
    }
}