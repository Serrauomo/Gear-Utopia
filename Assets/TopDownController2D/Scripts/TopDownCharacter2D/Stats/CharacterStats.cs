using System;
using TopDownCharacter2D.Attacks;
using UnityEngine;

namespace TopDownCharacter2D.Stats
{
    /// <summary>
    ///     The type of stat change
    /// </summary>
    public enum StatsChangeType
    {
        Add,
        Multiply,
        Override
    }

    [Serializable]
    public class CharacterStats
    {
        public StatsChangeType statsChangeType;

        [Range(0, 100)] [Tooltip("The max health of the character")]
        public int maxHealth;

        [Range(0f, 20f)] [Tooltip("The movement speed of the character")]
        public float speed;

        public AttackConfig attackConfig;
    }
}