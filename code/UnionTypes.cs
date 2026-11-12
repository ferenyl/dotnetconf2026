#!/usr/bin/env dotnet
#:property LangVersion=preview
// C# 15: union types
//
// A union is one type that holds a value of exactly one of a fixed set of "case types".
// The case types don't need a common base class or interface.
// The compiler knows all cases, so a switch over a union is checked for exhaustiveness:
// no default branch needed, and you get a warning if a case is missing.
//
// Run: dotnet run UnionTypes.cs -- 42 hello 7

using static System.Console;

// --- 1. Union of primitives: an API that accepts either a string or an int -----------------
foreach (var arg in args)
{
    // Implicit conversion from each case type into the union
    Volume volume = int.TryParse(arg, out var number) ? new Volume(number) : new Volume(arg);

    // Pattern match on the case types. Exhaustive: remove a branch and you get a warning.
    WriteLine(volume switch
    {
        int i => $"int: {i}",
        string s => $"string: {s}",
    });
}

// --- 2. Union of unrelated domain types ----------------------------------------------------
// Trip and RTTrip come from different systems and share no base type,
// but can still live in the same list via the Trips union.
List<Trips> trips = [
    new Trip(10, DateTimeOffset.Now),
    new RTTrip("20", DateTimeOffset.Now)
];

// Alternative: switch inline at every call site.
// Prefer putting the logic as a member on the union instead (see Trips.Desc below).
// foreach (var item in trips)
// {
//     (int id, DateTimeOffset date) = item switch
//     {
//         Trip t => (t.Id, t.Date),
//         RTTrip rt => (int.Parse(rt.SamtransId), rt.TripDate)
//     };
//
//     WriteLine($"Trip {id} {date}");
// }
foreach (var item in trips)
{
    WriteLine($"Trip {item.Desc}");
}

public record Trip(int Id, DateTimeOffset Date);
public record RTTrip(string SamtransId, DateTimeOffset TripDate);

// Union declarations: `union Name(CaseType1, CaseType2, ...)`.
// A union is a struct by default (so it's sealed). It can have methods and properties,
// but no extra state of its own.
public union Volume(string, int);

public union Trips(Trip, RTTrip)
{
    // `this switch` over the case types, written once on the union instead of at every call site
    public string Desc => this switch
    {
        Trip t => $"{t.Id} {t.Date}",
        RTTrip rt => $"{rt.SamtransId} {rt.TripDate}"
    };
};
