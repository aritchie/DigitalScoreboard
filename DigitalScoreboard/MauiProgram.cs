using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
            .UsePrismApp<App>(prism => prism
				.RegisterTypes(registry =>
				{
					registry.RegisterForNavigation<NavigationPage>();
					registry.RegisterForNavigation<MainPage, MainViewModel>();
                    registry.RegisterForNavigation<SettingsPage, SettingsViewModel>();
					registry.RegisterForNavigation<RefereePage, RefereeViewModel>();
                    registry.RegisterForNavigation<ScoreboardPage, ScoreboardViewModel>();
                })
				.OnAppStart(
					async navigator => await navigator.Navigate("NavigationPage/MainPage")
				)
            )
            .UseShiny()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("DS-DIGI.TTF", "Digital");
				fonts.AddFont("electron.ttf", "electron");
			});

        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddShinyService<AppSettings>();
		builder.Services.AddBluetoothLE();
		builder.Services.AddBluetoothLeHosting();

		return builder.Build();
	}
}
