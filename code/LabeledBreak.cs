#!/usr/bin/env dotnet
#:property LangVersion=preview
// C# 15: labeled break and continue
//
// Put a label on a loop (`outer: for ...`) and `break outer;` / `continue outer;`
// jump straight out of / to the next iteration of THAT loop from inside a nested loop.
// Replaces the "set a flag, break, check the flag, break again" pattern.
// Visual Studio has a quick fix (Ctrl+.) that rewrites the flag pattern for you.
//
// Run: dotnet run LabeledBreak.cs

using static System.Console;

// Find the first negative number in a 2D grid

int[][] grid =
[
    [1, 2, 3],
    [4, -1, 6],
    [7, 8, 9],
];

// Before: flag variable + extra check after the inner loop
bool found = false;
for (int row = 0; row < grid.Length; row++)
{
    for (int col = 0; col < grid[row].Length; col++)
    {
        if (grid[row][col] < 0)
        {
            WriteLine($"Old way: negative at ({row}, {col})");
            found = true;
            break;
        }
    }
    if (found) break;
}

// After: labeled break jumps straight out of the outer loop
outer: for (int row = 0; row < grid.Length; row++)
{
    for (int col = 0; col < grid[row].Length; col++)
    {
        if (grid[row][col] < 0)
        {
            WriteLine($"New way: negative at ({row}, {col})");
            break outer;
        }
    }
}

// Labeled continue: skip the rest of a row as soon as it contains a negative
rows: for (int row = 0; row < grid.Length; row++)
{
    int sum = 0;
    foreach (var value in grid[row])
    {
        if (value < 0) continue rows;
        sum += value;
    }
    WriteLine($"Row {row} sum: {sum}");
}
