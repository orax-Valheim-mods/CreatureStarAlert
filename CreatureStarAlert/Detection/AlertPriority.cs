namespace CreatureStarAlert.Detection
{
    /// <summary>
    /// Ranks the detected creatures for a same alert. The active criterion comes first
    /// (the most stars when the option is on, the nearest otherwise) and the distance
    /// always breaks ties, so the zone scan and the banner agree on
    /// which creature the player is shown.
    /// </summary>
    public static class AlertPriority
    {
        /// <summary>
        /// Negative when the left creature wins over the right one, positive when it
        /// loses, 0 when both rank the same.
        /// </summary>
        public static int Compare(
            int leftStars,
            float leftDistance,
            int rightStars,
            float rightDistance,
            bool preferMostStars
        )
        {
            if (preferMostStars && leftStars != rightStars)
            {
                return rightStars.CompareTo(leftStars);
            }

            return leftDistance.CompareTo(rightDistance);
        }
    }
}
