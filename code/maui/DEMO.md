# MAUI-demo: `dotnet run` från terminalen + agent som klickar i appen

Målet är att visa två saker:

1. En MAUI-app kan byggas, installeras och startas på en Android-emulator med `dotnet run` utan IDE.
2. En AI-agent (Claude Code) kan trycka på en knapp i den körande appen och kontrollera resultatet via DevFlow.

Kedjan ser ut så här:

```
Claude Code → MCP-server (maui devflow mcp) → broker (på värden) → DevFlow-agent i appen (bara Debug)
```

---

## Del A: engångsuppsättning (Linux)

### A1. Miljövariabler (i `~/.zshrc`)

```sh
export ANDROID_HOME="$HOME/Android/Sdk"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export JAVA_HOME="/usr/lib/jvm/default"
export PATH="$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$ANDROID_HOME/emulator:$PATH"
```

### A2. Android command-line tools (`sdkmanager`)

Installera via Android Studio: **Settings → Android SDK → SDK Tools → Android SDK Command-line Tools (latest)**.

Om du i stället installerar AUR-paketet `android-sdk-cmdline-tools-latest` hamnar verktygen i `/opt` och behöver symlänkas in:

```sh
ln -s /opt/android-sdk/cmdline-tools/latest ~/Android/Sdk/cmdline-tools/latest
```

Kontrollera sedan att allt är grönt:

```sh
maui doctor --platform android
```

### A3. Systemavbild och emulator

```sh
# --sdk_root behövs om sdkmanager kommer från /opt (AUR)
sdkmanager --sdk_root="$ANDROID_HOME" "system-images;android-36;google_apis;x86_64"
sdkmanager --sdk_root="$ANDROID_HOME" --licenses

maui android emulator create dotnetconf-pixel --package "system-images;android-36;google_apis;x86_64" --device pixel_8
```

Kräver att `/dev/kvm` är tillgänglig. Kontrollera med `emulator -accel-check`.

### A4. MAUI CLI och AI-skills

Detta är redan gjort i repot (`.claude/skills`, `.mcp.json` och `.maui/`):

```sh
dotnet tool install -g Microsoft.Maui.Cli --prerelease
maui ai init --env Claude
```

`.mcp.json` i repots rot registrerar MCP-servern `maui-devflow` (`maui devflow mcp`).

---

## Del B: förbered appen (görs en gång)

### B1. Lägg till DevFlow-agenten

```sh
cd code/maui
dotnet add package Microsoft.Maui.DevFlow.Agent --prerelease
```

### B2. Registrera agenten i `MauiProgram.cs`, bara i Debug

```csharp
using Microsoft.Maui.DevFlow.Agent;

// i CreateMauiApp(), före return builder.Build();
#if DEBUG
builder.AddMauiDevFlowAgent();
#endif
```

Agenten startar en liten HTTP-server inne i appen. Den ska aldrig följa med i Release.

### B3. Ge knappen ett stabilt `AutomationId`

Då kan agenten hitta knappen via namn i stället för via text eller koordinater. I `MainPage.xaml`:

```xml
<Button
    x:Name="CounterBtn"
    AutomationId="CounterBtn"
    Text="Click me"
    ... />
```

### B4. Aktivera MCP-servern i Claude Code

Starta `claude` i repots rot och godkänn `maui-devflow` när frågan dyker upp. Kontrollera sedan med `/mcp` att den är ansluten.

Servern är redan godkänd i `.claude/settings.local.json`:

```json
{ "enabledMcpjsonServers": ["random", "maui-devflow"] }
```

Tänk på att Claude Code läser in MCP-servrar vid start. Starta om `claude` efter ändringen.

### Varning vid bygget: XA0033

Arch versionsmärker OpenJDK som `21.0.13.0.3`, och Android-SDK:n kan inte tolka det. Därför kommer varningen XA0033 "Failed to get the Java SDK version". Den är ofarlig och bygget går igenom ändå.

---

## Del C: själva demon

### C1. Starta emulatorn

```sh
maui android emulator start dotnetconf-pixel
adb devices          # vänta tills "emulator-5554  device" syns
```

### C2. Starta brokern och öppna porten från emulatorn till värden

```sh
maui devflow broker start
adb reverse tcp:19223 tcp:19223     # låter agenten i appen registrera sig hos brokern
```

### C3. Visa poängen: kör appen från terminalen

```sh
cd code/maui
dotnet run -f net11.0-android --list-devices
dotnet run -f net11.0-android --device emulator-5554
```

`dotnet run` sköter hela kedjan: välja enhet, bygga, installera, starta appen och visa loggar.

Ange alltid `-f`. Utan det försöker `dotnet run` och `dotnet test` köra alla target frameworks.

Variant med hot reload:

```sh
dotnet watch -f net11.0-android --device emulator-5554
```

Om en ändring inte kan hot-reloadas säger watch det, och då startar Ctrl+R om appen.

### C4. Koppla CLI:t till agenten (i en ny terminal)

```sh
maui devflow wait                    # väntar tills appens agent har registrerat sig
maui devflow list                    # visar agenten och dess port (10223–10899)
adb forward tcp:<port> tcp:<port>    # porten från listan
maui devflow diagnose                # broker, agent och projekt ska vara OK
```

Kontrollera manuellt att det fungerar:

```sh
maui devflow ui tree --depth 3
maui devflow ui tap --automationId CounterBtn
maui devflow ui assert --automationId CounterBtn Text "Clicked 1 time"
```

### C5. Be agenten testa appen

Exempel på prompt i Claude Code (i repots rot):

> Appen i code/maui körs på Android-emulatorn. Använd DevFlow och tryck på knappen med AutomationId `CounterBtn` två gånger. Kontrollera att texten blir "Clicked 2 times" och ta en skärmdump som bevis.

Agenten använder då DevFlow-verktygen via MCP: läser det visuella trädet, trycker via `AutomationId`, läser `Text`-propertyn och tar en skärmdump.

Det här är bättre än bara skärmdumpar, eftersom agenten får det riktiga trädet. Det ger tydligare resultat och kostar färre tokens.

---

## Felsökning

| Problem | Lösning |
|---|---|
| `maui devflow list` är tom | Kontrollera att `adb reverse tcp:19223 tcp:19223` gjordes efter att emulatorn startat, att appen är byggd i **Debug** och att `AddMauiDevFlowAgent()` finns. |
| CLI:t når inte agenten | Kontrollera att `adb forward tcp:<port> tcp:<port>` använder porten från `maui devflow list`. |
| `No Android devices found` | Emulatorn har inte bootat klart. Kör `adb devices`. |
| MCP-verktygen syns inte i Claude | Kör `/mcp` i Claude Code och kontrollera att `maui-devflow` är godkänd. |
| Loggar från agenten | `adb logcat -s MauiDevFlow` |

Port-forwardingen gäller per emulatorsession. Starta om emulatorn, så måste C2 och `adb forward` i C4 göras igen.
