using System;
using System.Collections.Generic;
using CreatureStarAlert.Config;
using UnityEngine;

namespace CreatureStarAlert.Detection
{
    /// <summary>
    /// Scans the creatures currently loaded around the player and reports the watched
    /// ones, best first, so the most interesting creatures are alerted first. Every
    /// loaded creature is evaluated on each scan; the manager owns the anti-spam rules,
    /// this class only owns the "what is loaded right now" view and forgets the
    /// creatures that are not instantiated anymore, so that loading a zone again raises
    /// a new alert.
    /// </summary>
    public sealed class LoadedCreatureScanner
    {
        private readonly IDetectionConfig _config;
        private readonly IAlertManager _manager;
        private readonly HashSet<ZDOID> _loadedIds = new HashSet<ZDOID>();
        private readonly List<Character> _ranked = new List<Character>();
        private readonly Comparison<Character> _bestFirst;

        private float _accumulator;

        public LoadedCreatureScanner(IDetectionConfig config, IAlertManager manager)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));

            // Built once but reads the config on every call, so the order follows the
            // setting changed in game.
            _bestFirst = (left, right) =>
                AlertPriority.Compare(
                    left.GetLevel() - 1,
                    DistanceToPlayer(left),
                    right.GetLevel() - 1,
                    DistanceToPlayer(right),
                    _config.PreferMostStars
                );
        }

        /// <summary>Forgets the loaded creatures; called when leaving a world.</summary>
        public void Reset()
        {
            _loadedIds.Clear();
            _ranked.Clear();
            _accumulator = 0f;
        }

        public void Tick(float deltaTime)
        {
            _accumulator += deltaTime;
            if (_accumulator < _config.ScanIntervalSeconds)
            {
                return;
            }

            _accumulator = 0f;
            Scan();
        }

        private void Scan()
        {
            _loadedIds.Clear();
            _ranked.Clear();
            foreach (Character character in Character.GetAllCharacters())
            {
                if (character == null)
                {
                    continue;
                }

                ZDOID id = character.GetZDOID();
                if (id.IsNone())
                {
                    continue;
                }

                _loadedIds.Add(id);

                if (_config.IncludeLoadedCreatures)
                {
                    _ranked.Add(character);
                }
            }

            // Best first: a herd then warns about its winning member instead of
            // whichever creature the game happened to enumerate first.
            _ranked.Sort(_bestFirst);
            foreach (Character character in _ranked)
            {
                _manager.Report(character, AlertReason.Loaded);
            }

            // Creatures that unloaded are forgotten, so loading them again alerts.
            if (_config.ReAlertOnZoneLoad)
            {
                _manager.ForgetUnavailable(_loadedIds);
            }
        }

        private static float DistanceToPlayer(Character character)
        {
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return float.MaxValue;
            }

            return Vector3.Distance(player.transform.position, character.transform.position);
        }
    }
}
