using CommunityToolkit.Maui;
using DigitalScoreboard.Infrastructure;
using Microsoft.Extensions.Configuration;

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
                        //registry.RegisterForNavigation<NavigationPage>();
                        registry.RegisterForNavigation<MainPage, MainViewModel>();
                        registry.RegisterForNavigation<SettingsPage, SettingsViewModel>();
                        registry.RegisterForNavigation<RefereePage, RefereeViewModel>();
                        registry.RegisterForNavigation<ScoreboardPage, ScoreboardViewModel>();
                        registry.RegisterForNavigation<FullTimerPage, FullTimerViewModel>();
                    })
                    .OnAppStart("NavigationPage/MainPage")
            )
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("DS-DIGI.TTF", "Digital");
				fonts.AddFont("electron.ttf", "electron");
			});

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Configuration.AddJsonPlatformBundle(optional: false);
        builder.Services.AddSingleton(builder.Configuration.GetSection("Bluetooth").Get<BluetoothConfig>());

        builder.Services.AddSingleton<ScreenOrientation>();
        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddShinyService<AppSettings>();

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

        builder.Services.AddBluetoothLE();
		builder.Services.AddBluetoothLeHosting();

		return builder.Build();
	}
}
