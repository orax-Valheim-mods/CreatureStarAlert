namespace CreaturesStarsWatcher.Config
{
    /// <summary>
    /// Settings consumed by the detection pipeline.
    /// The configuration is split into small interfaces (Interface Segregation) so that
    /// every component only depends on the values it actually uses.
    /// </summary>
    public interface IDetectionConfig
    {
        /// <summary>Master switch: when false nothing is detected and no alert is ever produced.</summary>
        bool Enabled { get; }

        /// <summary>Logs every detection decision. Verbose, intended for troubleshooting only.</summary>
        bool DebugLogging { get; }

        /// <summary>Minimum number of stars required to raise an alert. Comparison is "greater than or equal".</summary>
        int StarsMinimum { get; }

        /// <summary>
        /// Maximum distance in metres between the local player and the creature. 0 disables the check:
        /// every loaded creature is considered. The game unloads zones beyond its simulation distance,
        /// so a creature is never instantiated farther than a few hundred metres.
        /// </summary>
        float MaxDistance { get; }

        /// <summary>Whether creatures already present when a zone is loaded or entered are considered.</summary>
        bool IncludeLoadedCreatures { get; }

        /// <summary>
        /// Forget the alert of a creature when it is unloaded, so that loading its zone
        /// again raises a new alert. When false, a creature is only alerted once per world session.
        /// </summary>
        bool ReAlertOnZoneLoad { get; }

        /// <summary>
        /// Minimum delay in seconds between two alerts. A nearer creature is not held back.
        /// 0 disables the cooldown.
        /// </summary>
        float CooldownSeconds { get; }

        /// <summary>How often, in seconds, loaded zones are scanned for star creatures.</summary>
        float ScanIntervalSeconds { get; }
    }

    /// <summary>Pattern list used to decide which creatures are watched.</summary>
    public interface ICreatureFilterConfig
    {
        /// <summary>Comma separated creature names, '*' wildcard allowed, case insensitive.</summary>
        string CreaturePatterns { get; }
    }

    /// <summary>Settings consumed by the banner UI.</summary>
    public interface IBannerConfig
    {
        /// <summary>How long a banner stays on screen, in seconds.</summary>
        float DurationSeconds { get; }

        /// <summary>Where the banner is drawn.</summary>
        BannerPosition Position { get; }

        /// <summary>Whether the live distance to the creature is displayed.</summary>
        bool ShowDistance { get; }

        /// <summary>Maximum number of alerts waiting to be displayed. The closest ones are kept.</summary>
        int MaxQueuedAlerts { get; }

        /// <summary>Font size of the banner in pixels. 0 = automatic, scaled to the screen height.</summary>
        int FontSize { get; }
    }
}
