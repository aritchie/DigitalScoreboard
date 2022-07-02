using DigitalScoreboard.Infrastructure;

namespace DigitalScoreboard;


public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp
			.CreateBuilder()
			//.UsePrismApp<App>(
			//	prism =>
			//	{

			//	}
			//)
            .UseShiny()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("DS-DIGI.TTF", "Digital");
			});

		builder.Services.AddShinyService<AppSettings>();
		builder.Services.AddBluetoothLE();
		builder.Services.AddBluetoothLeHosting();

		return builder.Build();
	}
}
