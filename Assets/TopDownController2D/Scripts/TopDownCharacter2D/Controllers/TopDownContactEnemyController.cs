using UnityEngine;

namespace TopDownCharacter2D.Controllers
{
    /// <summary>
    ///     A contact enemy AI, this ai simply tries to go to the position of the nearest target under a
    ///     certain distance to touch it and deal damage via a ChangeHealthOnTouch component.
    /// </summary>
    public class TopDownContactEnemyController : TopDownEnemyController
    {
        [SerializeField] [Range(0f, 100f)] private float followRange;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            Vector2 direction = Vector2.zero;
            if (DistanceToTarget() < followRange)
            {
                direction = DirectionToTarget();
            }

            OnMoveEvent.Invoke(direction);
        }
    }
}