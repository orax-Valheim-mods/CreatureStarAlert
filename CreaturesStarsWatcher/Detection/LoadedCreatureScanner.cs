using System;
using System.Collections.Generic;
using CreaturesStarsWatcher.Config;

namespace CreaturesStarsWatcher.Detection
{
    /// <summary>
    /// Periodically scans the creatures currently loaded around the player and reports
    /// the ones never seen before. This is what turns "a star creature is already in
    /// this zone" into an alert, the spawn patch covering the fresh spawn case.
    /// </summary>
    public sealed class LoadedCreatureScanner
    {
        private readonly IDetectionConfig _config;
        private readonly IAlertManager _manager;
        private readonly HashSet<ZDOID> _seen = new HashSet<ZDOID>();

        private float _accumulator;

        public LoadedCreatureScanner(IDetectionConfig config, IAlertManager manager)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        /// <summary>Forgets which creatures were seen; called when leaving a world.</summary>
        public void Reset()
        {
            _seen.Clear();
            _accumulator = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!_config.IncludeLoadedCreatures)
            {
                return;
            }

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
            foreach (Character character in Character.GetAllCharacters())
            {
                if (character == null)
                {
                    continue;
                }

                ZDOID id = character.GetZDOID();
                if (id.IsNone() || _seen.Contains(id))
                {
                    continue;
                }

                AlertOutcome outcome = _manager.Report(character, AlertReason.Loaded);

                // Out of range and disabled creatures stay eligible: they are evaluated
                // again on the next scan, once the player is closer or the mod re-enabled.
                if (outcome == AlertOutcome.OutOfRange || outcome == AlertOutcome.Disabled)
                {
                    continue;
                }

                _seen.Add(id);
            }
        }
    }
}
