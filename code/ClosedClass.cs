#:property LangVersion=preview

using static System.Console;

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

WriteLine(Compute(new Add(new Number(19), new Number(23))));

public closed record class Operation;

public sealed record class Number(int Value) : Operation;
public sealed record class Add(Operation Op1, Operation Op2) : Operation;
public sealed record class Subtract(Operation Op1, Operation Op2) : Operation;
public sealed record class Multiply(Operation Op1, Operation Op2) : Operation;
public sealed record class Divide(Operation Op1, Operation Op2) : Operation;
