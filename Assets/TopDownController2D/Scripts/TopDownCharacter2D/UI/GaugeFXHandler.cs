using UnityEngine;

namespace TopDownCharacter2D.UI
{
    /// <summary>
    ///     Handles the visual effects of an health gauge
    /// </summary>
    public class GaugeFXHandler : MonoBehaviour
    {
        private static readonly int PlayEffect = Animator.StringToHash("PlayEffect");
        [SerializeField] private Animator effectAnimator;

        /// <summary>
        ///     Starts the gauge effect
        /// </summary>
        public void StartGaugeFX()
        {
            effectAnimator.SetTrigger(PlayEffect);
        }
    }
}