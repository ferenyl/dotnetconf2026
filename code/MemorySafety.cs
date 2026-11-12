#!/usr/bin/env dotnet
#:property LangVersion=preview
#:property Features=$(Features);updated-memory-safety-rules
#:property AllowUnsafeBlocks=true
// C# 15: updated memory safety rules, "unsafe v2" (preview, GA planned for .NET 12)
//
// Old model: `unsafe` = "pointers are used here". Callers of an unsafe method never knew.
// New model: `unsafe` = "memory the runtime doesn't manage is accessed here".
//   - Declaring pointers, taking addresses and `fixed` are now allowed in safe code.
//   - Dereferencing still requires an unsafe context.
//   - `unsafe` on a METHOD makes it "requires-unsafe": every caller must be in an
//     unsafe context too, so the safety obligation is visible at the call site (like Rust).
//   - `unsafe` on the signature no longer makes the body unsafe; use an inner unsafe block.
//   - New `unsafe(expr)` expression for one-liners.
//
// Run: dotnet run MemorySafety.cs
//
// Note: the slides showed <MemorySafetyRulesVersion>, that property isn't implemented yet.
// The preview opt-in is the "updated-memory-safety-rules" compiler feature above.
//
// Block agents from sneaking in unsafe code: set AllowUnsafeBlocks=false and every
// unsafe block/member/expression fails with error CS0227.

using static System.Console;

byte[] buffer = [0x2A, 0x00, 0x00, 0x00, 0xFF];

// Pointers and fixed are now allowed in safe code, only dereferencing is unsafe
fixed (byte* p = buffer)
{
    // Calling a requires-unsafe member needs an unsafe context.
    // Remove the unsafe(...) to get: error CS9362 "'Native.ReadInt32(byte*)' must be used
    // in an unsafe context because it is marked as 'unsafe'"
    int value = unsafe(Native.ReadInt32(p));
    WriteLine($"ReadInt32 via unsafe expression: {value}");

    unsafe
    {
        WriteLine($"ReadInt32 via unsafe block: {Native.ReadInt32(p + 1)}");
    }
}

// The safe wrapper validates the contract, so callers don't need unsafe
WriteLine($"SafeRead(buffer, 0): {Native.SafeRead(buffer, 0)}");
try
{
    Native.SafeRead(buffer, 3);
}
catch (ArgumentOutOfRangeException ex)
{
    WriteLine($"SafeRead(buffer, 3): {ex.Message}");
}

static class Native
{
    /// <summary>Reads an int from raw memory.</summary>
    /// <remarks>Safety: <paramref name="source"/> must point to at least 4 readable bytes.</remarks>
    public static unsafe int ReadInt32(byte* source)
    {
        // `unsafe` on the signature does NOT make the body unsafe anymore
        unsafe
        {
            return *(int*)source;
        }
    }

    // Safe boundary: validates the obligation and discharges it
    public static int SafeRead(byte[] data, int offset)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, data.Length - sizeof(int));
        fixed (byte* p = &data[offset])
        {
            return unsafe(ReadInt32(p));
        }
    }
}
