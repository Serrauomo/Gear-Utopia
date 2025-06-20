using TopDownCharacter2D.Health;
using UnityEngine;

namespace TopDownCharacter2D.Items
{
    /// <summary>
    ///     Handles the logic of a health pickup item
    /// </summary>
    public class PickupHeal : PickupItem
    {
        [Tooltip("The amount of health restored when picked up")]
        [SerializeField] private float healAmount;

        protected override void OnPickedUp(GameObject receiver)
        {
            HealthSystem healthSystem = receiver.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.ChangeHealth(healAmount);
            }
        }
    }
}