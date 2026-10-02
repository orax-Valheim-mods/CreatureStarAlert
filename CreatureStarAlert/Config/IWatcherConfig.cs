using UnityEngine;

namespace CreatureStarAlert.Config
{
    /// <summary>Criterion deciding which creature a multi creature alert is about.</summary>
    public interface IAlertPriorityConfig
    {
        /// <summary>
        /// When true the creature with the most stars wins, otherwise the closest one.
        /// Distance always breaks ties.
        /// </summary>
        bool PreferMostStars { get; }
    }

    /// <summary>
    /// Settings consumed by the detection pipeline.
    /// The configuration is split into small interfaces (Interface Segregation) so that
    /// every component only depends on the values it actually uses.
    /// </summary>
    public interface IDetectionConfig : IAlertPriorityConfig
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
    public interface IBannerConfig : IAlertPriorityConfig
    {
        /// <summary>
        /// How long a banner stays on screen, in seconds. 0 = until the creature
        /// is unloaded or dies.
        /// </summary>
        float DurationSeconds { get; }

        /// <summary>Point of the banner that sits at the position.</summary>
        BannerAnchor Anchor { get; }

        /// <summary>Horizontal position of the anchor, in percent of the screen width (0 = left edge, 100 = right edge).</summary>
        int PositionX { get; }

        /// <summary>Vertical position of the anchor, in percent of the screen height (0 = top edge, 100 = bottom edge).</summary>
        int PositionY { get; }

        /// <summary>Whether PositionX and PositionY are percents of the screen or pixels.</summary>
        PositionUnit PositionUnit { get; }

        /// <summary>Whether the live distance to the creature is displayed.</summary>
        bool ShowDistance { get; }

        /// <summary>Colour of the creature name in the banner text.</summary>
        Color NameColor { get; }

        /// <summary>Colour of the stars in the banner text.</summary>
        Color StarColor { get; }

        /// <summary>Colour of the distance in the banner text.</summary>
        Color DistanceColor { get; }

        /// <summary>Whether the rectangle behind the banner text is drawn.</summary>
        bool ShowRectangle { get; }

        /// <summary>Opacity of the rectangle behind the text, in percent (0 = invisible, 100 = opaque).</summary>
        int RectangleOpacity { get; }

        /// <summary>Maximum number of alerts waiting to be displayed. The best ones on the criterion are kept.</summary>
        int MaxQueuedAlerts { get; }

        /// <summary>Font size of the banner in pixels. 0 = automatic, scaled to the screen height.</summary>
        int FontSize { get; }
    }
}
