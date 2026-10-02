using System.Text;
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

        private const string PositionTopLeft = "TopLeft";
        private const string PositionTopCenter = "TopCenter";
        private const string PositionTopRight = "TopRight";
        private const string PositionLeft = "Left";
        private const string PositionCenter = "Center";
        private const string PositionRight = "Right";
        private const string PositionBottomLeft = "BottomLeft";
        private const string PositionBottomCenter = "BottomCenter";
        private const string PositionBottomRight = "BottomRight";

        /// <summary>
        /// Upper bound of the MaxDistance setting. The game unloads zones beyond its simulation
        /// distance (2 zones of 64 m with the default settings, 5 zones at most), so a creature is
        /// never instantiated farther than a few hundred metres: asking for more would be the same
        /// as 0.
        /// </summary>
        private const float MaxLoadedCreatureDistance = 500f;

        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<bool> _debugLogging;
        private readonly ConfigEntry<int> _starsMinimum;
        private readonly ConfigEntry<string> _creaturePatterns;
        private readonly ConfigEntry<float> _maxDistance;
        private readonly ConfigEntry<bool> _includeLoadedCreatures;
        private readonly ConfigEntry<bool> _reAlertOnZoneLoad;
        private readonly ConfigEntry<float> _cooldownSeconds;
        private readonly ConfigEntry<float> _scanIntervalSeconds;
        private readonly ConfigEntry<float> _durationSeconds;
        private readonly ConfigEntry<string> _position;
        private readonly ConfigEntry<bool> _showDistance;
        private readonly ConfigEntry<int> _maxQueuedAlerts;
        private readonly ConfigEntry<int> _fontSize;
        private readonly ConfigEntry<bool> _preferMostStars;

        public PluginConfig(ConfigFile file)
        {
            _enabled = Bind(
                file,
                GeneralSection,
                nameof(Enabled),
                true,
                "Enable the creature star alerts."
            );

            _debugLogging = Bind(
                file,
                GeneralSection,
                nameof(DebugLogging),
                false,
                "Log every detection decision. Verbose, intended for troubleshooting only."
            );

            _starsMinimum = Bind(
                file,
                DetectionSection,
                nameof(StarsMinimum),
                2,
                new ConfigDescription(
                    "Minimum number of stars required to raise an alert (2 = 2 stars and above).",
                    new AcceptableValueRange<int>(0, 9)
                )
            );

            _creaturePatterns = Bind(
                file,
                DetectionSection,
                nameof(CreaturePatterns),
                "*",
                "Comma separated list of creatures to watch. Accepts the prefab name (for example 'Moose') "
                    + "or the localised display name. Supports the '*' wildcard and is case insensitive. "
                    + "'*' watches every creature."
            );

            _maxDistance = Bind(
                file,
                DetectionSection,
                nameof(MaxDistance),
                100f,
                new ConfigDescription(
                    "Maximum distance in metres between the player and the creature for an alert. "
                        + "0 = no distance filter: every creature the game has loaded is considered. "
                        + "The game unloads zones and their creatures beyond its simulation distance "
                        + "(about 200 m with the default settings), so the setting stops at 500 m: "
                        + "a larger value could never make a difference.",
                    new AcceptableValueRange<float>(0f, MaxLoadedCreatureDistance)
                )
            );

            _includeLoadedCreatures = Bind(
                file,
                DetectionSection,
                nameof(IncludeLoadedCreatures),
                true,
                "Also alert for star creatures found when entering or loading a zone, not only on fresh spawns."
            );

            _reAlertOnZoneLoad = Bind(
                file,
                DetectionSection,
                nameof(ReAlertOnZoneLoad),
                true,
                "Forget the alert of a creature when it unloads, so loading its zone again raises a new alert. "
                    + "When false, each creature is only alerted once per world session."
            );

            _cooldownSeconds = Bind(
                file,
                DetectionSection,
                nameof(CooldownSeconds),
                10f,
                new ConfigDescription(
                    "Minimum delay in seconds between two alerts. A nearer creature is not "
                        + "held back. 0 = no delay.",
                    new AcceptableValueRange<float>(0f, 300f)
                )
            );

            _scanIntervalSeconds = Bind(
                file,
                DetectionSection,
                nameof(ScanIntervalSeconds),
                1f,
                new ConfigDescription(
                    "How often, in seconds, loaded zones are scanned for star creatures.",
                    new AcceptableValueRange<float>(0.25f, 30f)
                )
            );

            _durationSeconds = Bind(
                file,
                AlertSection,
                nameof(DurationSeconds),
                6f,
                new ConfigDescription(
                    "How long the banner stays on screen, in seconds. 0 = keep it on "
                        + "screen until the creature is unloaded or dies.",
                    new AcceptableValueRange<float>(0f, 120f)
                )
            );

            _position = Bind(
                file,
                AlertSection,
                nameof(Position),
                PositionTopCenter,
                new ConfigDescription(
                    "Where the banner is drawn.",
                    new AcceptableValueList<string>(
                        PositionTopLeft,
                        PositionTopCenter,
                        PositionTopRight,
                        PositionLeft,
                        PositionCenter,
                        PositionRight,
                        PositionBottomLeft,
                        PositionBottomCenter,
                        PositionBottomRight
                    )
                )
            );

            _showDistance = Bind(
                file,
                AlertSection,
                nameof(ShowDistance),
                true,
                "Show the live distance to the creature in the banner."
            );

            _maxQueuedAlerts = Bind(
                file,
                AlertSection,
                nameof(MaxQueuedAlerts),
                3,
                new ConfigDescription(
                    "Maximum number of alerts waiting to be displayed. The best ones on "
                        + "the criterion are kept when it is full.",
                    new AcceptableValueRange<int>(1, 10)
                )
            );

            _fontSize = Bind(
                file,
                AlertSection,
                nameof(FontSize),
                0,
                new ConfigDescription(
                    "Font size of the banner in pixels. 0 = automatic (scaled to the screen "
                        + "height).",
                    new AcceptableValueRange<int>(0, 120)
                )
            );

            _preferMostStars = Bind(
                file,
                AlertSection,
                nameof(PreferMostStars),
                false,
                "When several star creatures are detected, show the one with the most stars "
                    + "instead of the closest one. Ties are always broken by distance."
            );
        }

        public bool Enabled => _enabled.Value;

        public bool DebugLogging => _debugLogging.Value;

        public int StarsMinimum => _starsMinimum.Value;

        public float MaxDistance => _maxDistance.Value;

        public bool IncludeLoadedCreatures => _includeLoadedCreatures.Value;

        public bool ReAlertOnZoneLoad => _reAlertOnZoneLoad.Value;

        public float CooldownSeconds => _cooldownSeconds.Value;

        public float ScanIntervalSeconds => _scanIntervalSeconds.Value;

        public string CreaturePatterns => _creaturePatterns.Value;

        public float DurationSeconds => _durationSeconds.Value;

        public BannerPosition Position => ParsePosition(_position.Value);

        public bool ShowDistance => _showDistance.Value;

        public int MaxQueuedAlerts => _maxQueuedAlerts.Value;

        public int FontSize => _fontSize.Value;

        public bool PreferMostStars => _preferMostStars.Value;

        /// <summary>
        /// Binds a setting under its readable key: the identifier given with nameof
        /// is turned into spaces and a single leading capital, so "DurationSeconds"
        /// is stored and displayed as "Duration seconds".
        /// </summary>
        private static ConfigEntry<T> Bind<T>(
            ConfigFile file,
            string section,
            string identifier,
            T defaultValue,
            string description
        )
        {
            return Bind(
                file,
                section,
                identifier,
                defaultValue,
                new ConfigDescription(description)
            );
        }

        private static ConfigEntry<T> Bind<T>(
            ConfigFile file,
            string section,
            string identifier,
            T defaultValue,
            ConfigDescription description
        )
        {
            return file.Bind(section, ReadableKey(identifier), defaultValue, description);
        }

        /// <summary>"DurationSeconds" becomes "Duration seconds".</summary>
        private static string ReadableKey(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return identifier;
            }

            StringBuilder key = new StringBuilder(identifier.Length + 4);
            key.Append(char.ToUpperInvariant(identifier[0]));
            for (int i = 1; i < identifier.Length; i++)
            {
                char character = identifier[i];
                if (char.IsUpper(character))
                {
                    key.Append(' ');
                }

                key.Append(char.ToLowerInvariant(character));
            }

            return key.ToString();
        }

        private static BannerPosition ParsePosition(string value)
        {
            switch (value?.Trim())
            {
                case PositionTopLeft:
                    return BannerPosition.TopLeft;
                case PositionTopRight:
                    return BannerPosition.TopRight;
                case PositionLeft:
                    return BannerPosition.Left;
                case PositionCenter:
                    return BannerPosition.Center;
                case PositionRight:
                    return BannerPosition.Right;
                case PositionBottomLeft:
                    return BannerPosition.BottomLeft;
                case PositionBottomCenter:
                    return BannerPosition.BottomCenter;
                case PositionBottomRight:
                    return BannerPosition.BottomRight;
                default: // PositionTopCenter, and anything unrecognised
                    return BannerPosition.TopCenter;
            }
        }
    }
}
