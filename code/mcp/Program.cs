// MCP C# SDK demo: a remote (Streamable HTTP) MCP server with elicitation.
//
// Created with `dotnet new mcpserver`. The tools in Tools/ ask the USER for input
// (elicitation) instead of letting the model guess parameters:
//   - RandomNumberTools: asks for min/max, then returns a random number
//   - TripOrderTools:    asks for from/to/departure, then places a fake trip order
//
// Run: dotnet run, then connect a client to http://localhost:5000/mcp
// (see .mcp.json) or debug with `npx @modelcontextprotocol/inspector`.

var builder = WebApplication.CreateBuilder(args);

// Add the MCP services: the transport to use (Streamable HTTP) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<TripOrderTools>();

var app = builder.Build();

// MCP endpoint: http://localhost:5000/mcp
app.MapMcp("/mcp");

app.Run();
