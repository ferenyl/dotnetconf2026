// Included transitively: app.cs -> Models/Trip.cs -> Formatting.cs
using System.Globalization;

public static class Formatting
{
    private static readonly CultureInfo Swedish = CultureInfo.GetCultureInfo("sv-SE");

    public static string Time(DateTimeOffset value) => value.ToString("HH:mm", Swedish);

    public static string Weekday(DateTimeOffset value) => value.ToString("dddd", Swedish);
}
