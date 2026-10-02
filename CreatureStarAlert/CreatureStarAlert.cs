using System.Reflection;
using BepInEx;
using CreatureStarAlert.Config;
using CreatureStarAlert.Detection;
using CreatureStarAlert.UI;
using HarmonyLib;
using UnityEngine;

namespace CreatureStarAlert
{
    /// <summary>
    /// Composition root of the mod: builds every component, wires them together and
    /// drives the work that must happen once per frame.
    /// </summary>
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    internal sealed class CreatureStarAlert : BaseUnityPlugin
    {
        public const string PluginGUID = "orax.CreatureStarAlert";
        public const string PluginName = "CreatureStarAlert";
        public const string PluginVersion = "0.1.0";

        internal static CreatureStarAlert Instance { get; private set; }

        /// <summary>Entry point used by the Harmony patches to report new star creatures.</summary>
        internal IAlertManager AlertManager { get; private set; }

        private Harmony _harmony;
        private LoadedCreatureScanner _scanner;
        private BannerUI _banner;
        private bool _wasInWorld;

        private void Awake()
        {
            Instance = this;

            var config = new PluginConfig(Config);
            ICreatureFilter filter = new WildcardCreatureFilter(config);
            _banner = new BannerUI(config, Logger);
            AlertManager = new AlertManager(config, filter, _banner, Logger);
            _scanner = new LoadedCreatureScanner(config, AlertManager);

            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo(
                $"{PluginName} {PluginVersion} loaded: stars >= {config.StarsMinimum}, "
                    + $"creatures '{config.CreaturePatterns}', radius "
                    + (config.MaxDistance > 0f ? $"{config.MaxDistance} m" : "no filter")
                    + $", priority {(config.PreferMostStars ? "most stars" : "closest")}"
            );
        }

        private void Update()
        {
            bool inWorld = Player.m_localPlayer != null;
            if (_wasInWorld && !inWorld)
            {
                // Left the world: alerts and seen creatures must not leak into the next session.
                AlertManager.Reset();
                _scanner.Reset();
                _banner.Clear();
            }

            _wasInWorld = inWorld;
            if (!inWorld)
            {
                return;
            }

            _scanner.Tick(Time.deltaTime);
            _banner.Tick(Time.deltaTime);
        }

        private void OnGUI()
        {
            _banner?.OnGui();
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
