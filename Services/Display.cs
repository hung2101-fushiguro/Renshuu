namespace TracNghiemWeb.Services;

public static class Display
{
    public static string Duration(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
        {
            time = TimeSpan.Zero;
        }

        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours} giờ {time.Minutes} phút";
        }

        if (time.TotalMinutes >= 1)
        {
            return $"{(int)time.TotalMinutes} phút {time.Seconds} giây";
        }

        return $"{time.Seconds} giây";
    }

    public static string Clock(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
        {
            time = TimeSpan.Zero;
        }

        return time.TotalHours >= 1
            ? time.ToString(@"h\:mm\:ss")
            : time.ToString(@"m\:ss");
    }

    public static string Percentage(double value) => $"{Math.Round(value, 1):0.#}%";
}
