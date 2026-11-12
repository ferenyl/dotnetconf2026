# .NET Conf 2026 – demos

Små demos av nyheter i C# 15 och .NET 11 från .NET Conf 2026.

Nästan alla demos är [file-based apps](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps): en enda `.cs`-fil utan projektfil. Varje fil börjar med en kommentar som förklarar vad den visar och hur man kör den.

## Krav

- .NET 11 SDK
- Många av funktionerna är i preview, så filerna sätter `#:property LangVersion=preview`.

## Köra en demo

```sh
cd code
dotnet run UnionTypes.cs

# eller direkt, tack vare shebang-raden (#!/usr/bin/env dotnet) på Linux/macOS
./UnionTypes.cs
```

## Demos

### C# 15

| Fil | Visar |
|---|---|
| [UnionTypes.cs](code/UnionTypes.cs) | **Unions**: en typ som håller exakt en av flera typer utan gemensamt arv, t.ex. `string` eller `int`, eller två olika trip-typer i samma lista. Switch-uttryck blir exhaustiva. Kör med argument: `dotnet run UnionTypes.cs -- 42 hello` |
| [ClosedClass.cs](code/ClosedClass.cs) | **Closed hierarchies**: `closed record class` kan bara ärvas i samma assembly. Kompilatorn känner då till alla subtyper, så switchen behöver ingen default-gren. |
| [CollectionExpressionsArguments.cs](code/CollectionExpressionsArguments.cs) | **Collection expression arguments**: `[with(...), a, b]` skickar argument till konstruktorn, t.ex. capacity eller en comparer (`StringComparer.OrdinalIgnoreCase`). |
| [LabeledBreak.cs](code/LabeledBreak.cs) | **Labeled break/continue**: `break outer;` och `continue rows;` i nästlade loopar, jämfört med det gamla flagg-mönstret. |
| [ExtensionIndexers.cs](code/ExtensionIndexers.cs) | **Extension indexers**: `sequence[3]`, `sequence[^1]` och `sequence[1..^1]` på vilken `IEnumerable<T>` som helst, via ett `extension`-block. Visar också att en instansindexer alltid vinner. |
| [MemorySafety.cs](code/MemorySafety.cs) | **Uppdaterade minnessäkerhetsregler ("unsafe v2")**: `unsafe` på en metod tvingar anroparen att också vara i en unsafe-kontext. Visar också nya `unsafe(expr)` och en säker wrapper som validerar kontraktet. Kommentaren förklarar hur `AllowUnsafeBlocks=false` stoppar agenter från att lägga in unsafe-kod. |

### .NET 11 runtime och bibliotek

| Fil | Visar |
|---|---|
| [DecimalFloatingPoint.cs](code/DecimalFloatingPoint.cs) | **IEEE-decimaler** `Decimal32/64/128`: `0.1 + 0.2 == 0.3`, quantum (1.0 jämfört med 1.00), `Quantize` till hela ören, division med noll och precision jämfört med `double`. |
| [NumericsExtras.cs](code/NumericsExtras.cs) | **Mindre numerics-nyheter**: `double.TryParsePartial("12.5ms")`, generiska `Random.NextInteger<T>`/`NextBinaryFloat<T>`, `Complex<T>` med `Decimal128` och `BFloat16` jämfört med `Half`. |
| [RuntimeAsync.cs](code/RuntimeAsync.cs) | **Runtime async** (preview): samma kod med compiler async (state machines) och med runtime async. Mäter tid, allokeringar per anrop och exceptions. Kör en gång, avkommentera `#:property Features=runtime-async=on` och kör igen. Kör med `-c Release`. |
| [PostQuantum.cs](code/PostQuantum.cs) | **Post-quantum-kryptering**: ML-KEM med Encapsulate/Decapsulate, sedan HKDF och AES-GCM. Visar också Composite ML-KEM (ML-KEM + X25519) och HPKE med single shot, flera meddelanden och manipulerade meddelanden. Kräver OpenSSL 3.5+ på Linux. |

### AI / MCP

| Mapp | Visar |
|---|---|
| [mcp/](code/mcp/) | **MCP C# SDK**: en remote MCP-server (Streamable HTTP) skapad med `dotnet new mcpserver`. Tools frågar *användaren* om indata via elicitation (nya MRTR-flödet i protokoll 2026-07-28, med fallback). `RandomNumberTools` frågar efter min/max och `TripOrderTools` lägger en fejkad resebeställning. Kör `dotnet run` i mappen och anslut till `http://localhost:5000/mcp`, eller använd `npx @modelcontextprotocol/inspector`. |

## Bra att veta

- **Runtime async**: `-p:Features=...` på kommandoraden fungerar inte för file-based apps. Använd `#:property`-direktivet.
- **Unsafe v2**: sliden visade `<MemorySafetyRulesVersion>`, men den egenskapen finns inte än. I preview slås det på med `Features=updated-memory-safety-rules`.
- **HPKE med ML-KEM** stöds inte på Linux/OpenSSL i mina tester. Demon faller då tillbaka på klassisk X25519. Vanlig ML-KEM och Composite ML-KEM fungerar.
- `CompositeMLKem` och `Hpke` är `[Experimental]` (`SYSLIB5006`/`SYSLIB5009`) i .NET 11.
