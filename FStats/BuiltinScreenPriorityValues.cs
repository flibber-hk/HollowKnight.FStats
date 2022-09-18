namespace FStats
{
    public static class BuiltinScreenPriorityValues
    {
        public static double TimeByAreaStat { get; } = -100_000;
        public static double ICChecksDisplay { get; } = -91_000;
        public static double ItemSyncData { get; } = -90_000;
        public static double ICChecksPerMinuteDisplay { get; } = -89_000;
        public static double HeroActionStats { get; } = -76_000;
        public static double DirectionalStats { get; } = -75_000;
        public static double CombatStats { get; } = -74_000;
        public static double SkillTimeline { get; } = -50_000;
        public static double BenchwarpStats { get; } = -25_000;
        public static double MiscStats { get; } = 50_000;
        public static double ExtensionStats { get; } = 100_000;

    }
}
