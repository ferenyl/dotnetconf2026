#!/usr/bin/env dotnet
#:property LangVersion=preview
// C# 15: collection expression arguments
//
// `[with(...), item1, item2]` passes arguments to the collection's constructor,
// so you can set capacity, a comparer, etc. without leaving collection expression syntax.
// Visual Studio has a quick fix (Ctrl+.) that converts `new HashSet<T>(comparer) { ... }` to this.
//
// Run: dotnet run CollectionExpressionsArguments.cs

using static System.Console;

string[] values = ["one", "two", "three"];

// Pass the capacity argument to the List<T> constructor, then spread the values in
List<string> names = [with(capacity: values.Length * 2), .. values];
WriteLine($"Capacity of names {names.Capacity}");

// Pass a comparer to the HashSet<T> constructor.
// The set ends up with ONE element, all three strings are equal with OrdinalIgnoreCase.
HashSet<string> set = [with(StringComparer.OrdinalIgnoreCase), "Hello", "HELLO", "hello"];
WriteLine("items in set:");
foreach (var item in set)
{
    WriteLine($"after OrdinalIgnoreCase {item}");
}

// Use case: case-insensitive blocklist for usernames at signup
HashSet<string> reservedNames = [with(StringComparer.OrdinalIgnoreCase), "admin", "root", "support"];

string[] signups = ["Alice", "ADMIN", "bob", "Root", "SuPpOrT"];
foreach (var name in signups)
{
    var status = reservedNames.Contains(name) ? "rejected (reserved)" : "ok";
    WriteLine($"{name}: {status}");
}

// Use case: leaderboard kept sorted descending via a custom comparer
SortedSet<int> highScores = [with(Comparer<int>.Create((a, b) => b.CompareTo(a))), 420, 1337, 99, 9001];
WriteLine($"Top score: {highScores.First()}");
WriteLine($"Leaderboard: {string.Join(", ", highScores)}");
