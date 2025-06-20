using UnityEngine;

namespace TopDownCharacter2D.Attacks.Melee
{
    /// <summary>
    ///     This class contains the configuration of a melee attack
    /// </summary>
    [CreateAssetMenu(fileName = "MeleeConfig", menuName = "TopDownController/Attacks/Melee", order = 0)]
    public class MeleeAttackConfig : AttackConfig
    {
        [Tooltip("The angle of the horizontal swing of the attack")]
        public float swingAngle;

        [Tooltip("The thrust distance of the attack")]
        public float thrustDistance;

        [Tooltip("The curve used to control the horizontal swing of the sword")]
        public AnimationCurve swingCurve;

        [Tooltip("The curve used to control the thrust position of the sword")]
        public AnimationCurve thrustCurve;
    }
}