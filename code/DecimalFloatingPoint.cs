#!/usr/bin/env dotnet
#:property LangVersion=preview
// .NET 11: IEEE 754 decimal floating point (Decimal32 / Decimal64 / Decimal128)
//
// Base-10 floating point types, unlike double/float which are base-2.
// Significant digits: Decimal32 = 7, Decimal64 = 16, Decimal128 = 34
// (System.Decimal has 28-29 but is not an IEEE type).
//
// The big idea is the "quantum": 1.0 and 1.00 are the same value but remember
// how many decimals they were written with (significant zeroes).
//
// Run: dotnet run DecimalFloatingPoint.cs

using System.Globalization;
using System.Numerics;
using static System.Console;

var inv = CultureInfo.InvariantCulture;

// --- 1. Base-10 math: 0.1 + 0.2 is exactly 0.3 -------------------------------------------
Decimal64 a = Decimal64.Parse("0.1", inv);
Decimal64 b = Decimal64.Parse("0.2", inv);
WriteLine($"Double sum: {0.1 + 0.2:R}");   // 0.30000000000000004
WriteLine($"Decimal64 sum: {a + b}");      // 0.3

// --- 2. Quantum: same value, different precision -----------------------------------------
Decimal64 oneDecimalPlace = Decimal64.Parse("1.0", inv);
Decimal64 twoDecimalPlaces = Decimal64.Parse("1.00", inv);
// == ignores quantum, so these are equal...
WriteLine($"Same value: {oneDecimalPlace == twoDecimalPlaces}");
// ...but they don't have the same quantum (0.1 vs 0.01)
WriteLine($"HaveSameQuantum: {Decimal64.HaveSameQuantum(oneDecimalPlace, twoDecimalPlaces)}");
WriteLine($"Quantum of 1.0: {Decimal64.GetQuantum(oneDecimalPlace)}");
WriteLine($"Quantum of 1.00: {Decimal64.GetQuantum(twoDecimalPlaces)}");

// --- 3. Quantize: round to a given quantum, e.g. whole cents -----------------------------
Decimal64 cents = Decimal64.Parse("0.01", inv);
foreach (string text in new[] { "1.005", "1.015", "2.5" })
{
    Decimal64 value = Decimal64.Parse(text, inv);
    WriteLine($"Quantize {text}: {Decimal64.Quantize(value, cents)}");
}

// --- 4. IEEE semantics: division by zero gives infinity instead of throwing ----------------
WriteLine($"IEEE divide by zero: {Decimal64.One / Decimal64.Zero}");

// --- 5. Precision: Decimal128 keeps digits that double loses ------------------------------
// 1 - 1e-30 becomes exactly 1.0 as a double, so Acos returns 0.
// Decimal128 keeps the difference and gives a real (tiny) angle.
Decimal128 nearOne = Decimal128.One - Decimal128.Parse("1e-30", inv);
WriteLine($"Double input after conversion: {(double)nearOne:R}");
WriteLine($"Double Acos: {Math.Acos((double)nearOne):R}");
WriteLine($"Decimal128 Acos: {Decimal128.Acos(nearOne)}");
