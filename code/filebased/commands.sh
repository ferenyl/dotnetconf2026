#!/usr/bin/env bash
# The CLI side of file-based apps in .NET 11. Run the whole script or copy single lines.
set -euo pipefail
cd "$(dirname "$0")"

echo "== 1. C# from stdin, no file at all"
# Fully isolated: nuget.config, Directory.Build.props etc. are NOT read.
# No directives means no MSBuild either, it goes straight to the compiler.
echo 'Console.WriteLine($"hello from stdin, {DateTime.Now:HH:mm}");' | dotnet run -

echo
echo "== 2. Build cache: first run builds, second run reuses the build"
# Cache key = hash of file path + SDK version. Changing code is cheap,
# changing directives (#:package etc.) is the expensive rebuild. --no-cache bypasses it.
dotnet clean include/app.cs > /dev/null
time dotnet run include/app.cs > /dev/null
time dotnet run include/app.cs > /dev/null

echo
echo "== 3. All dotnet commands understand file-based apps"
dotnet format scripts/weekinfo --verify-no-changes && echo "format: ok"
dotnet package list --file scripts/weekinfo --outdated
dotnet package list --file scripts/weekinfo --vulnerable
dotnet reference list --file project/app.cs

echo
echo "== 4. Script without .cs extension, run through its shebang"
chmod +x scripts/weekinfo
./scripts/weekinfo 2026-12-24
