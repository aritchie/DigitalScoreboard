namespace DigitalScoreboard;


public static class Constants
{
    public const string GameServiceUuid = "144340bf-3566-425e-98ff-e57aab8c6360";
    public const string GameCharacteristicUuid = "144340bf-3566-425e-98ff-e57aab8c6361";

    public static class BleIntents
    {
        public const byte HomeTeam = 0x01;
        public const byte AwayTeam = 0x02;

        public const byte Score = 0x01;
        public const byte IncrementDown = 0x02;
        public const byte IncrementPeriod = 0x03;
        public const byte TogglePlayClock = 0x04;
        public const byte TogglePeriodClock = 0x05;
        public const byte DecrementTimeout = 0x06;
        public const byte TogglePossession = 0x07;
        public const byte Ytg = 0x08;
        public const byte ClockTick = 0x11;

        public const byte SyncGame = 0x09;
        public const byte SyncRules = 0x10;
    }
}