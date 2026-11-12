#!/usr/bin/env -S dotnet --
// #:project - reference a regular .csproj from a file-based app
//
// Handy for scripts that reuse code from an existing solution.
// `dotnet reference add --file app.cs SharedLib/SharedLib.csproj` adds the directive for you (.NET 11).
//
// Note: app.cs lives NEXT TO SharedLib/, not inside it. A file-based app inside a
// project's folder picks up that project's build settings.
//
// Run: dotnet run app.cs -- 21.5
#:project SharedLib/SharedLib.csproj

using SharedLib;

double celsius = args.Length > 0 && double.TryParse(args[0], System.Globalization.CultureInfo.InvariantCulture, out var c) ? c : 20;
Console.WriteLine($"{celsius} °C = {Temperature.CelsiusToFahrenheit(celsius)} °F");
