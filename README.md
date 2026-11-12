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

### File-based apps i .NET 11 ([code/filebased/](code/filebased/))

| Fil | Visar |
|---|---|
| [include/app.cs](code/filebased/include/app.cs) | **`#:include`**: delar upp appen i flera filer. Includes är transitiva: `app.cs` → `Models/Trip.cs` → `Formatting.cs`. Inkluderade filer kan lägga till typer men inga top-level statements. |
| [ref/app.cs](code/filebased/ref/app.cs) | **`#:ref`**: refererar en annan file-based app, i det här fallet biblioteket [lib/greeter.cs](code/filebased/ref/lib/greeter.cs) med `OutputType=Library` och ett eget NuGet-paket. Biblioteket byggs som en egen assembly, som en `ProjectReference`. |
| [project/app.cs](code/filebased/project/app.cs) | **`#:project`**: refererar en vanlig `.csproj` ([SharedLib](code/filebased/project/SharedLib/)). Appen ligger bredvid projektmappen, inte i den. |
| [web/web.cs](code/filebased/web/web.cs) | **`#:sdk Microsoft.NET.Sdk.Web`**: ett helt web-API i en fil. [web.settings.json](code/filebased/web/web.settings.json) läses in automatiskt som appsettings och [web.run.json](code/filebased/web/web.run.json) fungerar som launchSettings (port 5050). |
| [scripts/weekinfo](code/filebased/scripts/weekinfo) | **Skript utan `.cs`-ändelse** med shebang (`#!/usr/bin/env -S dotnet --`). Kan symlänkas in på `PATH` och köras som ett vanligt kommando. Visar också metadata på direktiv (`#:package Humanizer.Core@2.* PrivateAssets=all`) och floating versions. |
| [commands.sh](code/filebased/commands.sh) | **CLI-sidan**: C# från stdin (`echo '...' \| dotnet run -`), byggcachen (0,9 s första körningen, 0,05 s med cache), `dotnet format`, `dotnet package list --outdated/--vulnerable` och `dotnet reference list` på file-based apps. |

### .NET MAUI

| Mapp | Visar |
|---|---|
| [maui/](code/maui/) | **`dotnet run` för MAUI och DevFlow**: appen byggs, installeras och startas på en Android-emulator med `dotnet run -f net11.0-android --device emulator-5554`, utan IDE. Med DevFlow-agenten (bara Debug) och MCP-servern `maui-devflow` kan en AI-agent läsa det visuella trädet, trycka på knappen via `AutomationId` och verifiera texten. Alla steg, även Linux-uppsättning och felsökning, finns i [DEMO.md](code/maui/DEMO.md). |

### AI / MCP

| Mapp | Visar |
|---|---|
| [mcp/](code/mcp/) | **MCP C# SDK**: en remote MCP-server (Streamable HTTP) skapad med `dotnet new mcpserver`. Tools frågar *användaren* om indata via elicitation (nya MRTR-flödet i protokoll 2026-07-28, med fallback). `RandomNumberTools` frågar efter min/max och `TripOrderTools` lägger en fejkad resebeställning. Kör `dotnet run` i mappen och anslut till `http://localhost:5000/mcp`, eller använd `npx @modelcontextprotocol/inspector`. |

## Bra att veta

- **Runtime async**: `-p:Features=...` på kommandoraden fungerar inte för file-based apps. Använd `#:property`-direktivet.
- **Unsafe v2**: sliden visade `<MemorySafetyRulesVersion>`, men den egenskapen finns inte än. I preview slås det på med `Features=updated-memory-safety-rules`.
- **HPKE med ML-KEM** stöds inte på Linux/OpenSSL i mina tester. Demon faller då tillbaka på klassisk X25519. Vanlig ML-KEM och Composite ML-KEM fungerar.
- **CA2266**: .NET 11 varnar om en file-based app inte börjar med `#!`, så alla entry points har en shebang.
- **Mappnamnet `bin/`** ignoreras av standard-`.gitignore`, så skript ligger i `scripts/`.
- `CompositeMLKem` och `Hpke` är `[Experimental]` (`SYSLIB5006`/`SYSLIB5009`) i .NET 11.
