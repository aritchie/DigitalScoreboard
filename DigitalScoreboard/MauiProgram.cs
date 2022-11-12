using CommunityToolkit.Maui;
using DigitalScoreboard.Infrastructure;
using DigitalScoreboard.Infrastructure.Impl;
using Prism.Controls;
using Prism.DryIoc;

namespace DigitalScoreboard;


public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
            .UseMauiApp<App>()
			.UseMauiCommunityToolkit()
            .UseShinyFramework(
                new DryIocContainerExtension(),
                prism => prism.OnAppStart("NavigationPage/MainPage")
            )
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("electron.ttf", "electron");
			});

#if DEBUG
        builder.Logging.AddDebug();
#endif
        RegisterServices(builder.Services);
        RegisterViews(builder.Services);


		return builder.Build();
	}


    static void RegisterServices(IServiceCollection s)
    {
        s.AddSingleton(DeviceDisplay.Current);
        s.AddShinyService<AppSettings>();
        s.AddBluetoothScoreboardServices();

        s.AddGlobalCommandExceptionHandler(new(
#if DEBUG
            ErrorAlertType.FullError
#else
            ErrorAlertType.NoLocalize
#endif
        ));
    }


    static void RegisterViews(IServiceCollection s)
    {
        s.RegisterForNavigation<MainPage, MainViewModel>();
        s.RegisterForNavigation<ScoreboardPage, ScoreboardViewModel>();
        s.RegisterForNavigation<ScanPage, ScanViewModel>();
        s.RegisterForNavigation<SettingsPage, SettingsViewModel>();
        s.RegisterForNavigation<FullTimerPage, FullTimerViewModel>();
    }
}
