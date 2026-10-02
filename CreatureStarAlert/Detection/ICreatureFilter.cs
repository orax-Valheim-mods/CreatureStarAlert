namespace CreatureStarAlert.Detection
{
    /// <summary>
    /// Decides whether a creature is watched. Implementations must be case insensitive
    /// and must treat a null or empty name as "not watched".
    /// </summary>
    public interface ICreatureFilter
    {
        /// <param name="creatureName">Prefab name or localised display name of the creature.</param>
        bool IsWatched(string creatureName);
    }
}
