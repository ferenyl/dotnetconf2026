namespace SharedLib;

/// <summary>Plain library code that a file-based app reuses via #:project.</summary>
public static class Temperature
{
    public static double CelsiusToFahrenheit(double celsius) => celsius * 9 / 5 + 32;
}
