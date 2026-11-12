#!/usr/bin/env -S dotnet --
// .NET 11: #:include - split a file-based app into several files
//
// `#:include` pulls another file into the compilation. Included .cs files can add
// types and methods but NOT top-level statements, there is still only one entry point.
// Includes are transitive: Models/Trip.cs includes ../Formatting.cs, so app.cs gets it too.
//
// Wildcards work as well (`#:include Models/*.cs`), but then build caching is disabled,
// so prefer literal paths.
//
// Run: dotnet run app.cs
#:include Models/Trip.cs

using static System.Console;

List<Trip> trips =
[
    new("Hem", "Skola", new DateTimeOffset(2026, 11, 13, 7, 45, 0, TimeSpan.FromHours(1))),
    new("Skola", "Hem", new DateTimeOffset(2026, 11, 13, 15, 10, 0, TimeSpan.FromHours(1))),
];

foreach (var trip in trips)
{
    // Trip comes from Models/Trip.cs, Formatting.Time from Formatting.cs (transitive include)
    WriteLine($"{Formatting.Time(trip.Departure)}  {trip.Describe()}");
}
