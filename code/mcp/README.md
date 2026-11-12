# MCP Server

## Om demon

En remote MCP-server (Streamable HTTP) byggd med MCP C# SDK och `dotnet new mcpserver`. Poängen är **elicitation**: i stället för att modellen gissar parametrar frågar toolet *användaren*.

| Tool | Gör |
|---|---|
| `GetRandomNumber` ([RandomNumberTools.cs](Tools/RandomNumberTools.cs)) | Frågar efter min och max och returnerar ett slumptal. |
| `PlaceTripOrder` ([TripOrderTools.cs](Tools/TripOrderTools.cs)) | Tar resenärens id som argument, frågar sedan efter från, till och avgångstid och lägger en fejkad beställning. |

Båda använder det nya MRTR-flödet (protokoll 2026-07-28). Servern returnerar ett "input required"-svar och klienten skickar om anropet med svaren, så det behövs inga sticky sessions. För äldre klienter finns en fallback till klassisk `ElicitAsync`.

### Köra demon

```sh
cd code/mcp
dotnet run                                  # lyssnar på http://localhost:5000/mcp
```

Testa på något av följande sätt:

- **MCP Inspector:** `npx @modelcontextprotocol/inspector`, anslut med Streamable HTTP till `http://localhost:5000/mcp` och anropa ett tool.
- **Claude Code:** starta `claude` i den här mappen. [.mcp.json](.mcp.json) registrerar servern som `random`. Prompta till exempel *"Ge mig ett slumptal"* eller *"Beställ en resa åt resenär 42"* och svara på frågorna som dyker upp.

Resten av den här filen kommer från mallen `dotnet new mcpserver`.

This README was created using the C# MCP server project template.
It demonstrates how you can easily create an MCP server using C# and publish it as a NuGet package.

The MCP server is built as a self-contained application and does not require the .NET runtime to be installed on the target machine.
However, since it is self-contained, it must be built for each target platform separately.
By default, the template is configured to build for:
* `win-x64`
* `win-arm64`
* `osx-arm64`
* `linux-x64`
* `linux-arm64`
* `linux-musl-x64`

If your users require more platforms to be supported, update the list of runtime identifiers in the project's `<RuntimeIdentifiers />` element.

See [aka.ms/nuget/mcp/guide](https://aka.ms/nuget/mcp/guide) for the full guide.

## Checklist before publishing to NuGet.org

- Test the MCP server locally using the steps below.
- Update the package metadata in the .csproj file, in particular the `<PackageId>`.
- Update `.mcp/server.json` to declare your MCP server's inputs.
  - See [configuring inputs](https://aka.ms/nuget/mcp/guide/configuring-inputs) for more details.
- Pack the project using `dotnet pack`.

The `bin/Release` directory will contain the package file (.nupkg), which can be [published to NuGet.org](https://learn.microsoft.com/nuget/nuget-org/publish-a-package).

## Developing locally

Start the server with `dotnet run` (it listens on `http://localhost:5000`, see `Properties/launchSettings.json`) and point your client at the MCP endpoint:

```json
{
  "servers": {
    "mcp": {
      "type": "http",
      "url": "http://localhost:5000/mcp"
    }
  }
}
```

Refer to the VS Code or Visual Studio documentation for more information on configuring and using MCP servers:

- [Use MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)
- [Use MCP servers in Visual Studio](https://learn.microsoft.com/visualstudio/ide/mcp-servers)

## Testing the MCP Server

Once configured, you can ask Copilot Chat for a random number, for example, `Give me 3 random numbers`. It should prompt you to use the `get_random_number` tool on the `mcp` MCP server and show you the results.

## Publishing to NuGet.org

1. Run `dotnet pack -c Release` to create the NuGet package
2. Publish to NuGet.org with `dotnet nuget push bin/Release/*.nupkg --api-key <your-api-key> --source https://api.nuget.org/v3/index.json`

## Using the MCP Server from NuGet.org

Once the MCP server package is published to NuGet.org, you can configure it in your preferred IDE. Both VS Code and Visual Studio use the `dnx` command to download and install the MCP server package from NuGet.org.

- **VS Code**: Create a `<WORKSPACE DIRECTORY>/.vscode/mcp.json` file
- **Visual Studio**: Create a `<SOLUTION DIRECTORY>\.mcp.json` file

For both VS Code and Visual Studio, the configuration file uses the following server definition:

```json
{
  "servers": {
    "mcp": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "<your package ID here>",
        "--version",
        "<your package version here>",
        "--yes"
      ]
    }
  }
}
```

## More information

.NET MCP servers use the [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) C# SDK. For more information about MCP:

- [Official Documentation](https://modelcontextprotocol.io/)
- [Protocol Specification](https://spec.modelcontextprotocol.io/)
- [GitHub Organization](https://github.com/modelcontextprotocol)
- [MCP C# SDK](https://csharp.sdk.modelcontextprotocol.io/)
