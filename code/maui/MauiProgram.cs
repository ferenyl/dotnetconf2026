// MAUI demo: `dotnet run` from the terminal + an AI agent tapping a button via DevFlow.
// The full step-by-step guide (Linux setup, emulator, broker, troubleshooting) is in DEMO.md.
//
// Run: dotnet run -f net11.0-android --device emulator-5554
//
// The only DevFlow-specific code in the app is the agent registration below
// and the AutomationId on the button in MainPage.xaml.
using Microsoft.Extensions.Logging;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();

		// DevFlow agent: lets AI agents/CLI read the visual tree, tap, screenshot etc.
		// via `maui devflow` and its MCP server. Debug only, never ship it in Release.
		builder.AddMauiDevFlowAgent();
#endif

		return builder.Build();
	}
}
