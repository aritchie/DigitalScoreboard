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
                prism => prism
                    .RegisterTypes(registry =>
                    {
                        registry.RegisterForNavigation<PrismNavigationPage>("NavigationPage");
                        registry.RegisterForNavigation<MainPage, MainViewModel>();
                        registry.RegisterForNavigation<ScoreboardPage, ScoreboardViewModel>();
                        registry.RegisterForNavigation<ScanPage, ScanViewModel>();
                        registry.RegisterForNavigation<SettingsPage, SettingsViewModel>();
                        registry.RegisterForNavigation<FullTimerPage, FullTimerViewModel>();
                    })
                    .OnAppStart("NavigationPage/MainPage")
            )
            .ConfigureFonts(fonts =>
			{
				//fonts.AddFont("DS-DIGI.TTF", "Digital");
				fonts.AddFont("electron.ttf", "electron");
			});

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddShinyService<AppSettings>();
        builder.Services.AddBluetoothScoreboardServices();

        builder.Services.AddGlobalCommandExceptionHandler(new(
#if DEBUG
            ErrorAlertType.FullError
#else
            ErrorAlertType.NoLocalize
#endif
        ));

		return builder.Build();
	}
}
