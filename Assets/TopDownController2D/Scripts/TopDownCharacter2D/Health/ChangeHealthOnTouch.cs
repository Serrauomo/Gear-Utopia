using UnityEngine;

namespace TopDownCharacter2D.Health
{
    /// <summary>
    ///     This script allows an entity to deal contact damage to other entities with a given tag
    /// </summary>
    public class ChangeHealthOnTouch : MonoBehaviour
    {
        [Tooltip("The health change on contact (must be negative for damages)")] [SerializeField]
        private float value;

        [Tooltip("The tag of the target of the health change")] [SerializeField]
        private string targetTag;

        private HealthSystem _collidingTargetHealthSystem;
        private TopDownKnockBack _collidingTargetKnockBackSystem;

        private bool _isCollidingWithTarget;

        private void FixedUpdate()
        {
            if (_isCollidingWithTarget)
            {
                ApplyHealthChange();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            GameObject receiver = collision.gameObject;

            if (!receiver.CompareTag(targetTag))
            {
                return;
            }

            _collidingTargetHealthSystem = receiver.GetComponent<HealthSystem>();
            if (_collidingTargetHealthSystem != null)
            {
                _isCollidingWithTarget = true;
            }

            _collidingTargetKnockBackSystem = receiver.GetComponent<TopDownKnockBack>();
        }


        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag(targetTag))
            {
                return;
            }

            _isCollidingWithTarget = false;
        }

        /// <summary>
        ///     Apply the change of health to the target
        /// </summary>
        private void ApplyHealthChange()
        {
            bool hasBeenChanged = _collidingTargetHealthSystem.ChangeHealth(value);

            if (_collidingTargetKnockBackSystem != null && hasBeenChanged)
            {
                _collidingTargetKnockBackSystem.ApplyKnockBack(transform);
            }
        }
    }
}