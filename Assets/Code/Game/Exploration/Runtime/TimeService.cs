using System;

namespace Code.Game.Exploration.Runtime
{
public sealed class TimeService
{
    private readonly int _minutesPerUnit;
    private int _currentUnits;

    public TimeService(int minutesPerUnit, int startUnits)
    {
        _minutesPerUnit = Math.Max(1, minutesPerUnit);
        _currentUnits = Math.Max(0, startUnits);
    }

    public int CurrentUnits => _currentUnits;

    public void AddUnits(int units)
    {
        _currentUnits = Math.Max(0, _currentUnits + units);
    }

    public string FormatCurrentTime()
    {
        return FormatTimeUnits(_currentUnits, _minutesPerUnit);
    }

    public static string FormatTimeUnits(int timeUnits, int minutesPerUnit)
    {
        var safeMinutesPerUnit = Math.Max(1, minutesPerUnit);
        var totalMinutes = Math.Max(0, timeUnits) * safeMinutesPerUnit;
        var minutesPerDay = 24 * 60;
        var dayIndex = totalMinutes / minutesPerDay;
        var dayMinutes = totalMinutes % minutesPerDay;
        var hours = dayMinutes / 60;
        var minutes = dayMinutes % 60;
        
        return $"Day {dayIndex + 1} {hours:00}:{minutes:00}";
    }
}
}