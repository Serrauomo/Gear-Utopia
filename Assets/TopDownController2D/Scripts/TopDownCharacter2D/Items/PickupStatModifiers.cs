using System.Collections.Generic;
using TopDownCharacter2D.Stats;
using UnityEngine;

namespace TopDownCharacter2D.Items
{
    /// <summary>
    ///     Handles the logic for a stat modifier pickup item
    /// </summary>
    public class PickupStatModifiers : PickupItem
    {
        [Tooltip("The stats modifier added to the character after this item is picked up")]
        [SerializeField] private List<CharacterStats> statsModifier;

        protected override void OnPickedUp(GameObject receiver)
        {
            CharacterStatsHandler statsHandler = receiver.gameObject.GetComponent<CharacterStatsHandler>();
            foreach (CharacterStats stat in statsModifier)
            {
                statsHandler.statsModifiers.Add(stat);
            }
        }
    }
}