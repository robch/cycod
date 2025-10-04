using System.ComponentModel;

public class DateAndTimeHelperFunctions
{
    [ReadOnly(true)]
    [Description("Gets the current date.")]
    public string GetCurrentDate()
    {
        var date = DateTime.Now;
        return $"{date.Year}-{date.Month}-{date.Day}";
    }

    [ReadOnly(true)]
    [Description("Gets the current time.")]
    public string GetCurrentTime()
    {
        var date = DateTime.Now;
        return $"{date.Hour}:{date.Minute}:{date.Second}";
    }
}