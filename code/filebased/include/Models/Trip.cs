// Included by ../app.cs. This file includes Formatting.cs in turn,
// which shows that #:include is transitive. Paths are relative to THIS file.
#:include ../Formatting.cs

public record Trip(string From, string To, DateTimeOffset Departure)
{
    public string Describe() => $"{From} -> {To} ({Formatting.Weekday(Departure)})";
}
