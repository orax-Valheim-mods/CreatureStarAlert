using HarmonyLib;

namespace CreatureStarAlert.Detection
{
    /// <summary>
    /// Fresh spawn detection.
    /// <see cref="Character.SetLevel"/> is the single place where the game assigns a level
    /// to a creature: SpawnSystem.Spawn only calls it when the rolled level is greater than 1,
    /// so it fires exactly when a star creature appears (spawns, births, summons included).
    /// Creatures that were already in the world are covered by <see cref="LoadedCreatureScanner"/>.
    /// </summary>
    [HarmonyPatch]
    internal static class CharacterSetLevelPatch
    {
        [HarmonyPatch(typeof(Character), nameof(Character.SetLevel))]
        private static void Postfix(Character __instance)
        {
            CreatureStarAlert.Instance?.AlertManager?.Report(__instance, AlertReason.Spawn);
        }
    }
}
