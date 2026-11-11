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
