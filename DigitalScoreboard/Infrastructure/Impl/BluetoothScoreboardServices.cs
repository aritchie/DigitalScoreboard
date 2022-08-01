namespace DigitalScoreboard.Infrastructure.Impl;

public static class BluetoothScoreboardServices
{
    public static void AddBluetoothScoreboardServices(this IServiceCollection services)
    {
        services.AddBluetoothLE();
        services.AddBleHostedCharacteristic<GameBleGattCharacteristic>();
        services.AddSingleton<IScoreboardManager, ScoreboardManager>();
    }
}
