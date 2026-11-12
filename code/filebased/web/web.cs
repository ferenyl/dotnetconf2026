#!/usr/bin/env -S dotnet --
// A whole web API in one file: #:sdk Microsoft.NET.Sdk.Web
//
// Sidecar files next to web.cs work like in a normal project:
//   web.settings.json - configuration (like appsettings.json)
//   web.run.json      - launch profiles (like Properties/launchSettings.json)
//
// Run:  dotnet run web.cs
// Try:  curl http://localhost:5050/
//       curl http://localhost:5050/trips/2
#:sdk Microsoft.NET.Sdk.Web
#:property PublishAot=false

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Values come from web.settings.json, loaded automatically, no AddJsonFile needed
string greeting = app.Configuration["Greeting"] ?? "Hello (no web.settings.json found)";
string[] stops = app.Configuration.GetSection("Stops").Get<string[]>() ?? [];

app.MapGet("/", () => new { greeting, environment = app.Environment.EnvironmentName });

app.MapGet("/trips/{id:int}", (int id) =>
    id >= 0 && id < stops.Length
        ? Results.Ok(new { id, stop = stops[id] })
        : Results.NotFound());

app.Run();
