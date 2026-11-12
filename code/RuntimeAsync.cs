#!/usr/bin/env dotnet
#:property LangVersion=preview
// .NET 11: runtime async (preview, expected default in .NET 12)
//
// Today the C# compiler rewrites every async method into a state machine class.
// With runtime async the runtime itself handles suspension instead: no generated
// state machines, (almost) no allocations when methods complete synchronously,
// faster exceptions and cleaner stacks in debuggers/profilers.
// Libraries should stay on compiler async in .NET 11 (Mono WASM lacks support), apps can opt in.
//
// Results on my machine (Release, Linux x64):
//   compiler async: ~50 ms, 43 bytes/iteration, exceptions 159 ms
//   runtime async:  ~33 ms,  0 bytes/iteration, exceptions  42 ms
//
// Run with `dotnet run -c Release RuntimeAsync.cs`, then uncomment the directive below
// and run again to compare. (-p:Features=... on the command line does NOT work for file-based apps.)
// #:property Features=runtime-async=on

using System.Diagnostics;
using static System.Console;

const int Iterations = 1_000_000;

// Runtime async methods are marked with MethodImplOptions.Async (0x2000) instead of
// getting a compiler-generated state machine class.
var level1 = typeof(Program).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
    .First(m => m.Name.Contains("Level1"));
bool runtimeAsync = ((int)level1.MethodImplementationFlags & 0x2000) != 0;
WriteLine($"Mode: {(runtimeAsync ? "runtime async" : "compiler async (state machines)")}");

// --- Hot path: 3 levels of async calls that almost always complete synchronously ---------
// Warm up so JIT/tiering doesn't skew the numbers
for (int i = 0; i < 10_000; i++) await Level1(i);

long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
var sw = Stopwatch.StartNew();
long total = 0;
for (int i = 0; i < Iterations; i++) total += await Level1(i);
sw.Stop();
long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

WriteLine($"Result: {total}");
WriteLine($"Time: {sw.ElapsedMilliseconds} ms for {Iterations:N0} iterations");
WriteLine($"Allocated: {allocated / (double)Iterations:F1} bytes per iteration");
WriteLine();

// Exceptions: throwing through async frames is where runtime async shines most
const int Throws = 20_000;
sw.Restart();
for (int i = 0; i < Throws; i++)
{
    try { await Level1(-1); }
    catch (InvalidOperationException) { }
}
sw.Stop();
WriteLine($"Exceptions: {sw.ElapsedMilliseconds} ms for {Throws:N0} throws through 3 async frames");

// Async methods that really suspend (Task.Yield) only on the first iteration,
// otherwise complete synchronously, the common hot-path case.
static async Task<int> Level1(int value) => await Level2(value) + 1;

static async Task<int> Level2(int value) => await Level3(value) + 1;

static async Task<int> Level3(int value)
{
    if (value < 0) throw new InvalidOperationException("Boom from Level3");
    if (value == 0) await Task.Yield();
    return value % 10;
}
