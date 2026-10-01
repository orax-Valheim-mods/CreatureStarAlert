using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;

namespace CreaturesStarsWatcher
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class CreaturesStarsWatcher : BaseUnityPlugin
    {
        public const string PluginGUID = "orax.CreaturesStarsWatcher";
        public const string PluginName = "CreaturesStarsWatcher";
        public const string PluginVersion = "0.1.0";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization =
            LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("CreaturesStarsWatcher has landed");

            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html
        }
    }
}
