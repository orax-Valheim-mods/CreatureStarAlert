using BepInEx.Configuration;

namespace CreaturesStarsWatcher.Config
{
    /// <summary>
    /// BepInEx backed implementation of the watcher settings.
    /// Values are read on every access, so changes applied in game (ConfigurationManager)
    /// or in the .cfg file take effect immediately, without restarting the game.
    /// </summary>
    public sealed class PluginConfig : IDetectionConfig, ICreatureFilterConfig, IBannerConfig
    {
        private const string GeneralSection = "General";
        private const string DetectionSection = "Detection";
        private const string AlertSection = "Alert";

        private const string PositionTopCenter = "TopCenter";
        private const string PositionCenter = "Center";
        private const string PositionBottomCenter = "BottomCenter";

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<bool> _debugLogging;
        private readonly ConfigEntry<int> _starsMinimum;
        private readonly ConfigEntry<string> _creaturePatterns;
        private readonly ConfigEntry<float> _maxDistance;
        private readonly ConfigEntry<bool> _includeLoadedCreatures;
        private readonly ConfigEntry<float> _cooldownSeconds;
        private readonly ConfigEntry<float> _scanIntervalSeconds;
        private readonly ConfigEntry<float> _durationSeconds;
        private readonly ConfigEntry<string> _position;
        private readonly ConfigEntry<bool> _showDistance;
        private readonly ConfigEntry<int> _maxQueuedAlerts;

        public PluginConfig(ConfigFile file)
        {
            _enabled = file.Bind(
                GeneralSection,
                nameof(Enabled),
                true,
                "Enable the creature star alerts."
            );

            _debugLogging = file.Bind(
                GeneralSection,
                nameof(DebugLogging),
                false,
                "Log every detection decision. Verbose, intended for troubleshooting only."
            );

            _starsMinimum = file.Bind(
                DetectionSection,
                nameof(StarsMinimum),
                2,
                new ConfigDescription(
                    "Minimum number of stars required to raise an alert (2 = 2 stars and above).",
                    new AcceptableValueRange<int>(0, 3)
                )
            );

            _creaturePatterns = file.Bind(
                DetectionSection,
                nameof(CreaturePatterns),
                "*",
                "Comma separated list of creatures to watch. Accepts the prefab name (for example 'Moose') "
                    + "or the localised display name. Supports the '*' wildcard and is case insensitive. "
                    + "'*' watches every creature."
            );

            _maxDistance = file.Bind(
                DetectionSection,
                nameof(MaxDistance),
                100f,
                new ConfigDescription(
                    "Maximum distance in metres between the player and the creature. 0 = unlimited.",
                    new AcceptableValueRange<float>(0f, 2000f)
                )
            );

            _includeLoadedCreatures = file.Bind(
                DetectionSection,
                nameof(IncludeLoadedCreatures),
                true,
                "Also alert for star creatures found when entering or loading a zone, not only on fresh spawns."
            );

            _cooldownSeconds = file.Bind(
                DetectionSection,
                nameof(CooldownSeconds),
                10f,
                new ConfigDescription(
                    "Minimum delay in seconds between two alerts. 0 = no delay.",
                    new AcceptableValueRange<float>(0f, 300f)
                )
            );

            _scanIntervalSeconds = file.Bind(
                DetectionSection,
                nameof(ScanIntervalSeconds),
                1f,
                new ConfigDescription(
                    "How often, in seconds, loaded zones are scanned for star creatures.",
                    new AcceptableValueRange<float>(0.25f, 30f)
                )
            );

            _durationSeconds = file.Bind(
                AlertSection,
                nameof(DurationSeconds),
                6f,
                new ConfigDescription(
                    "How long the banner stays on screen, in seconds.",
                    new AcceptableValueRange<float>(1f, 120f)
                )
            );

            _position = file.Bind(
                AlertSection,
                nameof(Position),
                PositionTopCenter,
                new ConfigDescription(
                    "Where the banner is drawn.",
                    new AcceptableValueList<string>(
                        PositionTopCenter,
                        PositionCenter,
                        PositionBottomCenter
                    )
                )
            );

            _showDistance = file.Bind(
                AlertSection,
                nameof(ShowDistance),
                true,
                "Show the live distance to the creature in the banner."
            );

            _maxQueuedAlerts = file.Bind(
                AlertSection,
                nameof(MaxQueuedAlerts),
                3,
                new ConfigDescription(
                    "Maximum number of alerts waiting to be displayed.",
                    new AcceptableValueRange<int>(1, 10)
                )
            );
        }

        public bool Enabled => _enabled.Value;

        public bool DebugLogging => _debugLogging.Value;

        public int StarsMinimum => _starsMinimum.Value;

        public float MaxDistance => _maxDistance.Value;

        public bool IncludeLoadedCreatures => _includeLoadedCreatures.Value;

        public float CooldownSeconds => _cooldownSeconds.Value;

        public float ScanIntervalSeconds => _scanIntervalSeconds.Value;

        public string CreaturePatterns => _creaturePatterns.Value;

        public float DurationSeconds => _durationSeconds.Value;

        public BannerPosition Position => ParsePosition(_position.Value);

        public bool ShowDistance => _showDistance.Value;

        public int MaxQueuedAlerts => _maxQueuedAlerts.Value;

        private static BannerPosition ParsePosition(string value)
        {
            switch (value?.Trim())
            {
                case PositionCenter:
                    return BannerPosition.Center;
                case PositionBottomCenter:
                    return BannerPosition.BottomCenter;
                default:
                    return BannerPosition.TopCenter;
            }
        }
    }
}
