using UnityEngine;

namespace CreatureStarAlert.Detection
{
    /// <summary>Why a creature was reported.</summary>
    public enum AlertReason
    {
        /// <summary>The creature was instantiated: spawn system, spawner, birth, summon...</summary>
        Spawn,

        /// <summary>The creature was already in the world and appeared during a zone scan.</summary>
        Loaded,
    }

    /// <summary>Immutable description of a detected star creature.</summary>
    public sealed class CreatureAlert
    {
        public CreatureAlert(
            string prefabName,
            string displayName,
            int stars,
            AlertReason reason,
            Character character,
            float distance
        )
        {
            PrefabName = prefabName;
            DisplayName = displayName;
            Stars = stars;
            Reason = reason;
            Character = character;
            Position = character.transform.position;
            Distance = distance;
        }

        /// <summary>Internal prefab name, for example "Moose". Used for filtering.</summary>
        public string PrefabName { get; }

        /// <summary>Localised name shown to the player, for example "Moose" or "Élan".</summary>
        public string DisplayName { get; }

        /// <summary>Number of stars: 1 for a one star creature, 2 for a two star creature.</summary>
        public int Stars { get; }

        public AlertReason Reason { get; }

        /// <summary>The creature itself. Becomes Unity-null once the creature is destroyed.</summary>
        public Character Character { get; }

        /// <summary>False once the creature has been destroyed or unloaded.</summary>
        public bool IsAvailable => Character != null;

        /// <summary>Position of the creature when the alert was raised (fallback once destroyed).</summary>
        public Vector3 Position { get; }

        /// <summary>Distance to the local player when the alert was raised.</summary>
        public float Distance { get; }
    }
}
