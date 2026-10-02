using System;
using System.Collections.Generic;
using BepInEx.Logging;
using CreaturesStarsWatcher.Config;
using UnityEngine;

namespace CreaturesStarsWatcher.Detection
{
    /// <summary>
    /// Applies the filters (stars, creature list, distance), the anti-spam rules
    /// (one alert per creature, cooldown) and forwards the resulting alert to the sink.
    /// </summary>
    public sealed class AlertManager : IAlertManager
    {
        private const string CloneSuffix = "(Clone)";

        private readonly IDetectionConfig _config;
        private readonly ICreatureFilter _filter;
        private readonly IAlertSink _sink;
        private readonly ManualLogSource _logger;

        private readonly HashSet<ZDOID> _alerted = new HashSet<ZDOID>();
        private float _lastAlertTime;
        private float _lastAlertDistance;

        public AlertManager(
            IDetectionConfig config,
            ICreatureFilter filter,
            IAlertSink sink,
            ManualLogSource logger
        )
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public AlertOutcome Report(Character character, AlertReason reason)
        {
            AlertOutcome outcome = Evaluate(character, reason, out CreatureAlert alert);

            // Only the candidates are logged: every loaded creature is evaluated on each
            // zone scan, and the others would quickly flood the log.
            if (_config.DebugLogging && character != null)
            {
                int stars = character.GetLevel() - 1;
                if (stars >= _config.StarsMinimum && outcome != AlertOutcome.AlreadyAlerted)
                {
                    _logger.LogDebug($"[StarsWatcher] {reason} '{character.name}': {outcome}");
                }
            }

            if (outcome == AlertOutcome.Alerted)
            {
                _sink.Enqueue(alert);
                _logger.LogInfo(
                    $"[StarsWatcher] {reason}: {alert.DisplayName} ({alert.PrefabName}) "
                        + $"with {alert.Stars} star(s) at {alert.Distance:0} m"
                );
            }

            return outcome;
        }

        public void Reset()
        {
            _alerted.Clear();
            _lastAlertTime = 0f;
            _lastAlertDistance = 0f;
        }

        public void ForgetUnavailable(IEnumerable<ZDOID> loadedCreatures)
        {
            if (loadedCreatures == null)
            {
                return;
            }

            _alerted.IntersectWith(loadedCreatures);
        }

        private AlertOutcome Evaluate(
            Character character,
            AlertReason reason,
            out CreatureAlert alert
        )
        {
            alert = null;

            if (!_config.Enabled)
            {
                return AlertOutcome.Disabled;
            }

            if (character == null || character.IsPlayer())
            {
                return AlertOutcome.NotWatched;
            }

            int stars = character.GetLevel() - 1;
            if (stars < _config.StarsMinimum)
            {
                return AlertOutcome.NotWatched;
            }

            ZDOID id = character.GetZDOID();
            if (id.IsNone())
            {
                return AlertOutcome.NotWatched;
            }

            if (_alerted.Contains(id))
            {
                return AlertOutcome.AlreadyAlerted;
            }

            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return AlertOutcome.OutOfRange;
            }

            float distance = Vector3.Distance(
                player.transform.position,
                character.transform.position
            );
            if (_config.MaxDistance > 0f && distance > _config.MaxDistance)
            {
                return AlertOutcome.OutOfRange;
            }

            string prefabName = GetPrefabName(character);
            string displayName = character.GetHoverName();
            if (!IsWatched(prefabName) && !IsWatched(displayName))
            {
                return AlertOutcome.NotWatched;
            }

            float now = Time.realtimeSinceStartup;
            bool coolingDown =
                _config.CooldownSeconds > 0f && now - _lastAlertTime < _config.CooldownSeconds;

            // A nearer creature breaks the cooldown: the closest one must reach the
            // banner first, and every creature is only alerted once anyway.
            if (coolingDown && distance >= _lastAlertDistance)
            {
                return AlertOutcome.Cooldown;
            }

            _alerted.Add(id);
            _lastAlertTime = now;
            _lastAlertDistance = distance;
            alert = new CreatureAlert(prefabName, displayName, stars, reason, character, distance);
            return AlertOutcome.Alerted;
        }

        private bool IsWatched(string creatureName)
        {
            return !string.IsNullOrEmpty(creatureName) && _filter.IsWatched(creatureName);
        }

        private static string GetPrefabName(Character character)
        {
            string name = character.name;
            if (name.EndsWith(CloneSuffix, StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - CloneSuffix.Length);
            }

            return name.TrimEnd();
        }
    }
}
