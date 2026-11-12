#!/usr/bin/env dotnet
#:property LangVersion=preview
// C# 15: closed hierarchies
//
// `closed` on a class/record means it can only be inherited from inside the same assembly.
// It is also implicitly abstract. Since nobody outside can add new subtypes, the compiler
// knows the full list of derived types, and a switch over them is exhaustive:
// no `_ => throw ...` default branch needed and no exhaustiveness warning.
//
// Good for public APIs: expose the hierarchy, but callers can't invent new cases.
//
// Run: dotnet run ClosedClass.cs

using static System.Console;

// A tiny expression evaluator. Note: no default branch in the switch.
// Add a new subtype of Operation below and you get a warning here until it's handled.
static int Compute(Operation op)
{
    return op switch
    {
        Number(var v) => v,
        Add(var op1, var op2) => Compute(op1) + Compute(op2),
        Subtract(var op1, var op2) => Compute(op1) - Compute(op2),
        Multiply(var op1, var op2) => Compute(op1) * Compute(op2),
        Divide(var op1, var op2) => Compute(op1) / Compute(op2),
    };
}

// (19 + 23) = 42
WriteLine(Compute(new Add(new Number(19), new Number(23))));

// The closed base type. Inheriting from it in another assembly is a compile error.
public closed record class Operation;

// All the cases. `sealed` so the hierarchy can't grow further down either.
public sealed record class Number(int Value) : Operation;
public sealed record class Add(Operation Op1, Operation Op2) : Operation;
public sealed record class Subtract(Operation Op1, Operation Op2) : Operation;
public sealed record class Multiply(Operation Op1, Operation Op2) : Operation;
public sealed record class Divide(Operation Op1, Operation Op2) : Operation;
