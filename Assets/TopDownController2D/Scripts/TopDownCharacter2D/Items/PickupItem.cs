using UnityEngine;
using UnityEngine.Events;

namespace TopDownCharacter2D.Items
{
    /// <summary>
    ///     Handles the logic of a pickup item
    /// </summary>
    public abstract class PickupItem : MonoBehaviour
    {
        [Tooltip("If the pickup item must be destroyed or not after having been picked up")]
        [SerializeField] private bool destroyOnPickup = true;
        
        [Tooltip("The layer of the objects that can pick up this item")]
        [SerializeField] private LayerMask canBePickupBy;

        public UnityEvent OnPickup { get; } = new UnityEvent();

        /// <summary>
        ///     If the item collides with a player, give its effect to the player and disappear itself
        /// </summary>
        /// <param name="other"> The other collider </param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (canBePickupBy.value == (canBePickupBy.value | (1 << other.gameObject.layer)))
            {
                OnPickedUp(other.gameObject);
                OnPickup.Invoke();
                if (destroyOnPickup)
                {
                    DestroyItem();
                }
            }
        }

        /// <summary>
        ///     Called when the item gets picked up
        /// </summary>
        /// <param name="receiver"> The GameObject receiving the effect </param>
        protected abstract void OnPickedUp(GameObject receiver);

        /// <summary>
        ///     Destroy the item while allowing it to finish its effects.
        /// </summary>
        private void DestroyItem()
        {
            foreach (Behaviour component in transform.GetComponentsInChildren<Behaviour>())
            {
                if (!(component is AudioSource))
                {
                    component.enabled = false;
                }
            }

            foreach (Renderer component in transform.GetComponentsInChildren<Renderer>())
            {
                if (!(component is ParticleSystemRenderer))
                {
                    component.enabled = false;
                }
            }

            Destroy(gameObject, 5f);
        }
    }
}