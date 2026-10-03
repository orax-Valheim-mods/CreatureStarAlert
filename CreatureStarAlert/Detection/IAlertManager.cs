using System;
using System.Collections.Generic;

namespace CreatureStarAlert.Detection
{
    /// <summary>Result of a report sent to the <see cref="IAlertManager"/>.</summary>
    public enum AlertOutcome
    {
        /// <summary>The mod is disabled.</summary>
        Disabled,

        /// <summary>The creature does not match the filters (player, stars, patterns...).</summary>
        NotWatched,

        /// <summary>Nothing to do now, but the creature may become interesting later.</summary>
        OutOfRange,

        /// <summary>An alert was already raised for this creature.</summary>
        AlreadyAlerted,

        /// <summary>The alert was produced and handed over to the sink.</summary>
        Alerted,
    }

    /// <summary>Turns characters into alerts, applying the filters and the anti-spam rules.</summary>
    public interface IAlertManager
    {
        /// <summary>
        /// Evaluates a character. Repeat calls for the same creature must stay harmless:
        /// loaded creatures are reported on every zone scan.
        /// </summary>
        /// <param name="character">Creature to evaluate.</param>
        /// <param name="reason">How the creature was noticed.</param>
        AlertOutcome Report(Character character, AlertReason reason);

        /// <summary>Forgets every alert raised during the current world session.</summary>
        void Reset();

        /// <summary>
        /// Forgets the alert of every creature missing from the given set (unloaded or
        /// out of the detection radius), so that they raise a new alert when they
        /// appear again (see the ReAlertOnZoneLoad setting).
        /// </summary>
        /// <param name="relevantCreatures">Creatures that keep their alert: instantiated and within the radius.</param>
        void ForgetUnavailable(IEnumerable<ZDOID> relevantCreatures);
    }
}
