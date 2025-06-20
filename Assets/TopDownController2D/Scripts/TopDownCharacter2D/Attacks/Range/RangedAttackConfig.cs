using UnityEngine;

namespace TopDownCharacter2D.Attacks.Range
{
    /// <summary>
    ///     This class contains the configuration of a ranged attack
    /// </summary>
    [CreateAssetMenu(fileName = "RangedAttackConfig", menuName = "TopDownController/Attacks/Range", order = 0)]
    public class RangedAttackConfig : AttackConfig
    {
        [Tooltip("The duration of a projectile before disappearing")]
        public float duration;

        [Tooltip("The maximum angle variation of the projectile")]
        public float spread;

        [Tooltip("The number of projectile shot per attack")]
        public int numberOfProjectilesPerShot;

        [Tooltip("The angle between each projectile shot (ignored when there are only one)")]
        public float multipleProjectilesAngle;

        [Tooltip("The color of the projectile's sprite")]
        public Color projectileColor;
    }
}