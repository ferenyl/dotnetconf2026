#!/usr/bin/env dotnet
#:property LangVersion=preview
using System.Globalization;
using System.Numerics;

Decimal64 a = Decimal64.Parse("0.1", CultureInfo.InvariantCulture);
Decimal64 b = Decimal64.Parse("0.2", CultureInfo.InvariantCulture);
Console.WriteLine($"Double sum: {0.1 + 0.2:R}");
Console.WriteLine($"Decimal64 sum: {a + b}");

Decimal64 oneDecimalPlace = Decimal64.Parse("1.0", CultureInfo.InvariantCulture);
Decimal64 twoDecimalPlaces = Decimal64.Parse("1.00", CultureInfo.InvariantCulture);
Console.WriteLine($"Same value: {oneDecimalPlace == twoDecimalPlaces}");
Console.WriteLine($"HaveSameQuantum: {Decimal64.HaveSameQuantum(oneDecimalPlace, twoDecimalPlaces)}");
Console.WriteLine($"Quantum of 1.0: {Decimal64.GetQuantum(oneDecimalPlace)}");
Console.WriteLine($"Quantum of 1.00: {Decimal64.GetQuantum(twoDecimalPlaces)}");

Decimal64 cents = Decimal64.Parse("0.01", CultureInfo.InvariantCulture);
foreach (string text in new[] { "1.005", "1.015", "2.5" })
{
    Decimal64 value = Decimal64.Parse(text, CultureInfo.InvariantCulture);
    Console.WriteLine($"Quantize {text}: {Decimal64.Quantize(value, cents)}");
}

Console.WriteLine($"IEEE divide by zero: {Decimal64.One / Decimal64.Zero}");

Decimal128 nearOne = Decimal128.One - Decimal128.Parse("1e-30", CultureInfo.InvariantCulture);
Console.WriteLine($"Double input after conversion: {(double)nearOne:R}");
Console.WriteLine($"Double Acos: {Math.Acos((double)nearOne):R}");
Console.WriteLine($"Decimal128 Acos: {Decimal128.Acos(nearOne)}");
