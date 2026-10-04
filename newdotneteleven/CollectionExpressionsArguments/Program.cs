string[] values = ["one", "two", "three"];

// Pass capacity argument to List<T> constructor
List<string> names = [with(capacity: values.Length * 2), .. values];
Console.WriteLine($"Capacity of names {names.Capacity}");

// Pass comparer argument to HashSet<T> constructor
HashSet<string> set = [with(StringComparer.OrdinalIgnoreCase), "Hello", "HELLO", "hello"];
// set contains only one element because all strings are equal with OrdinalIgnoreCase
Console.WriteLine("items in set:");
foreach (var item in set)
{
    Console.WriteLine(item);
}

// Use case: case-insensitive blocklist for usernames at signup
HashSet<string> reservedNames = [with(StringComparer.OrdinalIgnoreCase), "admin", "root", "support"];

string[] signups = ["Alice", "ADMIN", "bob", "Root", "SuPpOrT"];
foreach (var name in signups)
{
    var status = reservedNames.Contains(name) ? "rejected (reserved)" : "ok";
    Console.WriteLine($"{name}: {status}");
}

// Use case: leaderboard sorted descending via comparer
SortedSet<int> highScores = [with(Comparer<int>.Create((a, b) => b.CompareTo(a))), 420, 1337, 99, 9001];
Console.WriteLine($"Top score: {highScores.First()}");
Console.WriteLine($"Leaderboard: {string.Join(", ", highScores)}");
