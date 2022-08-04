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
			.UseMauiCommunityToolkit()
            .UseShinyFramework<App>(
                new DryIocContainerExtension(),
                prism => prism
                    .RegisterTypes(registry =>
                    {
                        registry.RegisterForNavigation<PrismNavigationPage>("NavigationPage");
                        registry.RegisterForNavigation<ScanPage, ScanViewModel>();
                        registry.RegisterForNavigation<MainPage, MainViewModel>();
                        registry.RegisterForNavigation<SettingsPage, SettingsViewModel>();
                        registry.RegisterForNavigation<ScoreboardPage, ScoreboardViewModel>();
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
        builder.Services.AddSingleton<ScreenOrientation>();
        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddShinyService<AppSettings>();
        builder.Services.AddBluetoothScoreboardServices();

        builder.Services.AddGlobalCommandExceptionHandler(options =>
        {
#if DEBUG
            options.AlertType = ErrorAlertType.FullError; // this will show the full error in a dialog during debug
            options.LogError = false;
#else
            options.AlertType = ErrorAlertType.NoLocalize;
            options.LogError = true;
#endif
        });


		return builder.Build();
	}
}
