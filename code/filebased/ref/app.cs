#!/usr/bin/env -S dotnet --
// .NET 11: #:ref - reference another file-based app (like a ProjectReference)
//
// lib/greeter.cs is a file-based library with its own directives and packages.
// #:ref builds it as a separate assembly and references it.
// Compare: #:include copies source into this compilation, #:project references a .csproj.
//
// Run: dotnet run app.cs -- Ada 3
#:ref lib/greeter.cs

string name = args.Length > 0 ? args[0] : "world";
int count = args.Length > 1 && int.TryParse(args[1], out var n) ? n : 1;

Console.WriteLine(Greeter.Hello(name, count));
