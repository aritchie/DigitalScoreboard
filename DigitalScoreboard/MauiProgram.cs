using DigitalScoreboard.Infrastructure;
using Microsoft.Extensions.Configuration;

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

		//builder.Configuration.AddJsonPlatformBundle(optional: false);
		//builder.Services.AddSingleton(builder.Configuration.GetSection("Bluetooth").Get<BluetoothConfig>());
		builder.Services.AddSingleton(new BluetoothConfig
		{
			ServiceUuid = "144340bf-3566-425e-98ff-e57aab8c6360",
			CharacteristicUuid = "144340bf-3566-425e-98ff-e57aab8c6361"
        });
        builder.Services.AddSingleton(DeviceDisplay.Current);
        builder.Services.AddShinyService<AppSettings>();
		builder.Services.AddBluetoothLE();
		builder.Services.AddBluetoothLeHosting();

		return builder.Build();
	}
}
