## Required Software

- Visual Studio 2026 (https://www.visualstudio.com/downloads/download-visual-studio-vs.aspx) or JetBrains Rider.
- .NET SDK 10 (https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- git

## Building from Source

Dashik targets `net10.0` and uses [Cake.Frosting](https://cakebuild.net/) for its build/publish pipeline.

```bash
# Run the app
dotnet run --project ./src/Dashik.Desktop/Dashik.Desktop.csproj

# Or use the build script to publish platform packages (Linux, macOS, Windows)
./build.sh   # macOS/Linux
./build.ps1  # Windows
```
