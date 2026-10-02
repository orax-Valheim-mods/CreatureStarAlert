namespace CreaturesStarsWatcher.Config
{
    /// <summary>
    /// Point of the banner placed at the configured position: the nine points of a
    /// three by three grid, from <see cref="TopLeft"/> to <see cref="BottomRight"/>.
    /// </summary>
    public enum BannerAnchor
    {
        /// <summary>Top left corner of the banner.</summary>
        TopLeft,

        /// <summary>Top middle edge of the banner.</summary>
        TopCenter,

        /// <summary>Top right corner of the banner.</summary>
        TopRight,

        /// <summary>Left middle edge of the banner.</summary>
        Left,

        /// <summary>Centre of the banner.</summary>
        Center,

        /// <summary>Right middle edge of the banner.</summary>
        Right,

        /// <summary>Bottom left corner of the banner.</summary>
        BottomLeft,

        /// <summary>Bottom middle edge of the banner.</summary>
        BottomCenter,

        /// <summary>Bottom right corner of the banner.</summary>
        BottomRight,
    }
}
