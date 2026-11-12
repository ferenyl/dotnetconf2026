#!/usr/bin/env dotnet
#:property LangVersion=preview
// .NET 11 numerics: the smaller additions (see DecimalFloatingPoint.cs for Decimal32/64/128)
//
//   1. double.TryParsePartial  - parse a number prefix, e.g. "12.5ms" -> 12.5 + "ms"
//   2. Generic Random          - NextInteger<T> / NextBinaryFloat<T> for any numeric type
//   3. Complex<T>              - complex numbers with any precision, not just double
//   4. BFloat16                - "brain float", truncated float used a lot in AI
//
// Run: dotnet run NumericsExtras.cs

using System.Globalization;
using System.Numerics;
using static System.Console;

var inv = CultureInfo.InvariantCulture;

// TryParsePartial: parse up to the first invalid char, get how many chars were read
foreach (var input in new[] { "12.5ms", "250ms", "3s", "abc" })
{
    if (double.TryParsePartial(input, NumberStyles.Float, inv, out double value, out int charsConsumed))
    {
        string unit = input[charsConsumed..];
        WriteLine($"{input,-7} -> value {value}, unit '{unit}'");
    }
    else
    {
        WriteLine($"{input,-7} -> not a number");
    }
}
WriteLine();

// Generic Random: one API for every integer / binary floating point type (fixed seed for repeatable output)
var random = new Random(42);
byte b = random.NextInteger<byte>();
long l = random.NextInteger<long>(1_000, 2_000);
Half h = random.NextBinaryFloat<Half>();
WriteLine($"byte: {b}, long: {l}, Half: {h}");
WriteLine();

// Complex<T>: same algorithm, choose precision by type
Complex<float> cf = new(1.5f, 2f);
Complex<Decimal128> cd = new(Decimal128.Parse("1.5", inv), Decimal128.Parse("2", inv));
WriteLine($"Complex<float>      squared: {cf * cf}");
WriteLine($"Complex<Decimal128> squared: {cd * cd}");
WriteLine();

// BFloat16: same range as float, much less precision (popular in AI weights).
// Compare with Half: Half is more precise but overflows to infinity much earlier.
WriteLine("BFloat16: 8 bit exponent, 7 bit mantissa. Half: 5 bit exponent, 10 bit mantissa.");
foreach (float original in new[] { 0.1f, 1000.7f })
{
    WriteLine($"float {original,-8} -> BFloat16 {(float)(BFloat16)original,-12} Half {(float)(Half)original}");
}

float big = 1e30f;
WriteLine($"1e30 as BFloat16: {(float)(BFloat16)big}, as Half: {(float)(Half)big}");
