using System;
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

        private PluginConfig _config;
        private Harmony _harmony;
        private LoadedCreatureScanner _scanner;
        private BannerUI _banner;
        private bool _wasInWorld;
        private bool _enabled;

        /// <summary>Set from the config thread, consumed on the main thread by <see cref="Update"/>.</summary>
        private volatile bool _detectionChanged;

        private void Awake()
        {
            Instance = this;

            _config = new PluginConfig(Config);
            _config.DetectionChanged += OnDetectionChanged;
            _enabled = _config.Enabled;
            ICreatureFilter filter = new WildcardCreatureFilter(_config);
            _banner = new BannerUI(_config, Logger);
            AlertManager = new AlertManager(_config, filter, _banner, Logger);
            _scanner = new LoadedCreatureScanner(_config, AlertManager);

            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo(
                $"{PluginName} {PluginVersion} loaded: stars >= {_config.StarsMinimum}, "
                    + $"creatures '{_config.CreaturePatterns}', radius "
                    + (_config.MaxDistance > 0f ? $"{_config.MaxDistance} m" : "no filter")
                    + $", priority {(_config.PreferMostStars ? "most stars" : "closest")}"
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
            ApplyEnabled();
            ApplyDetectionChanges();

            if (!inWorld || !_enabled)
            {
                return;
            }

            _scanner.Tick(Time.deltaTime);
            _banner.Tick(Time.deltaTime);
        }

        /// <summary>
        /// Takes the banner down as soon as the mod is disabled in game. No alert is
        /// reported anymore once it is off, so nothing else would remove the one on
        /// screen, and the creatures still around must alert again when it is turned
        /// back on.
        /// </summary>
        private void ApplyEnabled()
        {
            bool enabled = _config.Enabled;
            if (enabled == _enabled)
            {
                return;
            }

            _enabled = enabled;
            if (enabled)
            {
                return;
            }

            DropAlerts();
        }

        /// <summary>
        /// Drops the banner and forgets the alerts raised before a detection setting changed:
        /// a creature already alerted must alert again once it is watched, and the one on
        /// screen must leave as soon as the new settings do not watch it. The next scan then
        /// reports the creatures that qualify now.
        /// </summary>
        private void ApplyDetectionChanges()
        {
            if (!_detectionChanged)
            {
                return;
            }

            _detectionChanged = false;
            DropAlerts();
        }

        /// <summary>Drops the banner and forgets the alerts raised so far.</summary>
        private void DropAlerts()
        {
            AlertManager.Reset();
            _banner.Clear();
        }

        private void OnDetectionChanged(object sender, EventArgs e)
        {
            _detectionChanged = true;
        }

        private void OnGUI()
        {
            _banner?.OnGui();
        }

        private void OnDestroy()
        {
            if (_config != null)
            {
                _config.DetectionChanged -= OnDetectionChanged;
            }

            _harmony?.UnpatchSelf();
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
