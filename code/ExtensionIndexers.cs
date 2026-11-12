#!/usr/bin/env dotnet
#:property LangVersion=preview
// C# 15: extension indexers
//
// C# 14 added extension members (`extension(T source) { ... }` blocks) with properties
// and methods. C# 15 adds indexers, so you can write `sequence[3]` on any type.
//
// Run: dotnet run ExtensionIndexers.cs

using static System.Console;

IEnumerable<string> words = "the quick brown fox jumps over the lazy dog".Split(' ').Where(w => w.Length > 3);

// Indexers and a property on any IEnumerable<T>, not just lists/arrays
WriteLine($"Count: {words.Count}");
WriteLine($"First: {words[0]}");
WriteLine($"Last: {words[^1]}");
WriteLine($"Middle: {string.Join(", ", words[1..^1])}");

// Works on lazy sequences too (only enumerates what it needs)
IEnumerable<int> squares = Enumerable.Range(1, int.MaxValue).Select(i => i * i);
WriteLine($"10th square: {squares[9]}");

// Note: an instance indexer always wins over an extension indexer,
// so this can't add new overloads to e.g. string or List<T>.

static class Extensions
{
    // Everything inside this block extends IEnumerable<T>; `source` is the receiver
    extension<T>(IEnumerable<T> source)
    {
        // Extension property (C# 14)
        public int Count => source.Count();
        // Extension indexers (C# 15): int, ^from-end and ranges
        public T this[int index] => source.ElementAt(index);
        public T this[Index index] => source.ElementAt(index);
        public IEnumerable<T> this[Range range] => source.Take(range);
    }
}
