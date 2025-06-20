using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using TopDownCharacter2D.Attacks;
using TopDownCharacter2D.Attacks.Melee;
using TopDownCharacter2D.Attacks.Range;
using UnityEngine;

namespace TopDownCharacter2D.Stats
{
    /// <summary>
    ///     Handles the stats and their modification for a character
    /// </summary>
    public class CharacterStatsHandler : MonoBehaviour
    {
        [SerializeField] [Tooltip("The default stats of this character")]
        private CharacterStats baseStats;

        public readonly ObservableCollection<CharacterStats>
            statsModifiers = new ObservableCollection<CharacterStats>();

        public CharacterStats CurrentStats { get; private set; }

        private void Awake()
        {
            UpdateCharacterStats(null, null);
            statsModifiers.CollectionChanged += UpdateCharacterStats;
        }

        /// <summary>
        ///     Called when the list of stat modifiers changes, update the stats accordingly
        /// </summary>
        private void UpdateCharacterStats(object sender, NotifyCollectionChangedEventArgs e)
        {
            AttackConfig config = null;
            if (baseStats.attackConfig != null)
            {
                config = Instantiate(baseStats.attackConfig);
            }

            CurrentStats = new CharacterStats {attackConfig = config};
            UpdateStats((a, b) => b, baseStats);
            if (CurrentStats.attackConfig != null)
            {
                CurrentStats.attackConfig.target = baseStats.attackConfig.target;
            }

            foreach (CharacterStats modifier in statsModifiers.OrderBy(o => o.statsChangeType))
            {
                if (modifier.statsChangeType == StatsChangeType.Override)
                {
                    UpdateStats((o, o1) => o1, modifier);
                }
                else if (modifier.statsChangeType == StatsChangeType.Add)
                {
                    UpdateStats((o, o1) => o + o1, modifier);
                }
                else if (modifier.statsChangeType == StatsChangeType.Multiply)
                {
                    UpdateStats((o, o1) => o * o1, modifier);
                }
            }

            LimitAllStats();
        }

        /// <summary>
        ///     Updates the stats
        /// </summary>
        /// <param name="operation"> The operation to use to update the stats </param>
        /// <param name="newModifier"> The stat modifier to apply </param>
        private void UpdateStats(Func<float, float, float> operation, CharacterStats newModifier)
        {
            CurrentStats.maxHealth = (int) operation(CurrentStats.maxHealth, newModifier.maxHealth);
            CurrentStats.speed = operation(CurrentStats.speed, newModifier.speed);
            if (newModifier.attackConfig == null || CurrentStats.attackConfig == null)
            {
                return;
            }

            CurrentStats.attackConfig.delay =
                operation(CurrentStats.attackConfig.delay, newModifier.attackConfig.delay);
            CurrentStats.attackConfig.power =
                operation(CurrentStats.attackConfig.power, newModifier.attackConfig.power);
            CurrentStats.attackConfig.size = operation(CurrentStats.attackConfig.size, newModifier.attackConfig.size);
            CurrentStats.attackConfig.speed =
                operation(CurrentStats.attackConfig.speed, newModifier.attackConfig.speed);

            if (CurrentStats.attackConfig.GetType() != newModifier.attackConfig.GetType())
            {
                return;
            }

            switch (CurrentStats.attackConfig)
            {
                case RangedAttackConfig _:
                    ApplyRangedStats(operation,
                        newModifier); // This method is only called if the character uses a ranged weapon
                    break;
                case MeleeAttackConfig _:
                    ApplyMeleeStats(operation, newModifier);
                    break;
            }
        }

        /// <summary>
        ///     Limits all the stats to avoid problems
        /// </summary>
        private void LimitAllStats()
        {
            if (CurrentStats == null || CurrentStats.attackConfig == null)
            {
                return;
            }

            CurrentStats.attackConfig.delay =
                CurrentStats.attackConfig.delay < MinAttackDelay ? MinAttackDelay : CurrentStats.attackConfig.delay;
            CurrentStats.attackConfig.power = CurrentStats.attackConfig.power < MinAttackPower
                ? MinAttackPower
                : CurrentStats.attackConfig.power;
            CurrentStats.attackConfig.size = CurrentStats.attackConfig.size < MinAttackSize
                ? MinAttackSize
                : CurrentStats.attackConfig.size;
            CurrentStats.attackConfig.speed = CurrentStats.attackConfig.speed < MinAttackSpeed
                ? MinAttackSpeed
                : CurrentStats.attackConfig.speed;
            CurrentStats.speed = CurrentStats.speed < MinSpeed ? MinSpeed : CurrentStats.speed;
            CurrentStats.maxHealth = CurrentStats.maxHealth < MinMaxHealth ? MinMaxHealth : CurrentStats.maxHealth;
        }

        /// <summary>
        ///     Applies a ranged stats modifier
        /// </summary>
        /// <param name="operation"> The blend operation</param>
        /// <param name="newModifier"> The modifier</param>
        private void ApplyRangedStats(Func<float, float, float> operation, CharacterStats newModifier)
        {
            RangedAttackConfig currentRangedAttacks = (RangedAttackConfig) CurrentStats.attackConfig;

            if (!(newModifier.attackConfig is RangedAttackConfig))
            {
                return;
            }

            RangedAttackConfig rangedAttacksModifier = (RangedAttackConfig) newModifier.attackConfig;
            currentRangedAttacks.multipleProjectilesAngle =
                operation(currentRangedAttacks.multipleProjectilesAngle, rangedAttacksModifier.multipleProjectilesAngle);
            currentRangedAttacks.spread = operation(currentRangedAttacks.spread, rangedAttacksModifier.spread);
            currentRangedAttacks.duration = operation(currentRangedAttacks.duration, rangedAttacksModifier.duration);
            currentRangedAttacks.numberOfProjectilesPerShot = Mathf.CeilToInt(operation(currentRangedAttacks.numberOfProjectilesPerShot,
                rangedAttacksModifier.numberOfProjectilesPerShot));
            currentRangedAttacks.projectileColor = new Color(
                operation(currentRangedAttacks.projectileColor.r, rangedAttacksModifier.projectileColor.r),
                operation(currentRangedAttacks.projectileColor.g, rangedAttacksModifier.projectileColor.g),
                operation(currentRangedAttacks.projectileColor.b, rangedAttacksModifier.projectileColor.b),
                operation(currentRangedAttacks.projectileColor.a, rangedAttacksModifier.projectileColor.a));
        }
        
        /// <summary>
        ///     Applies a bullet stats modifier
        /// </summary>
        /// <param name="operation"> The blend operation</param>
        /// <param name="newModifier"> The modifier</param>
        private void ApplyMeleeStats(Func<float, float, float> operation, CharacterStats newModifier)
        {
            MeleeAttackConfig currentMeleeAttacks= (MeleeAttackConfig) CurrentStats.attackConfig;

            if (!(newModifier.attackConfig is MeleeAttackConfig))
            {
                return;
            }
            
            // NOTE: In case of a power up we ignore the curves
            
            MeleeAttackConfig meleeAttacksModifier = (MeleeAttackConfig) newModifier.attackConfig;
            currentMeleeAttacks.swingAngle =
                operation(currentMeleeAttacks.swingAngle, meleeAttacksModifier.swingAngle);
            
            currentMeleeAttacks.thrustDistance = 
                operation(currentMeleeAttacks.thrustDistance, meleeAttacksModifier.thrustDistance);
        }

        #region Stats limits

        private const float MinAttackDelay = 0.03f;
        private const float MinAttackPower = 0.5f;
        private const float MinAttackSize = 0.4f;
        private const float MinAttackSpeed = .1f;

        private const float MinSpeed = 1f;
        private const int MinMaxHealth = 1;

        #endregion
    }
}