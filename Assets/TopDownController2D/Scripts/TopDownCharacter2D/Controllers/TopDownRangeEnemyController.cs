using UnityEngine;

namespace TopDownCharacter2D.Controllers
{
    /// <summary>
    ///     A simple ranged enemy AI, this AI will simply try to get close enough to the player and then shoot in its
    ///     direction
    /// </summary>
    public class TopDownRangeEnemyController : TopDownEnemyController
    {
        [SerializeField] private float followRange = 15f;
        [SerializeField] private float shootRange = 10f;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            float distance = DistanceToTarget();
            Vector2 direction = DirectionToTarget();

            IsAttacking = false;
            if (distance <= followRange)
            {
                if (distance <= shootRange)
                {
                    int layerMaskTarget = Stats.CurrentStats.attackConfig.target;
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 11f,
                        (1 << LayerMask.NameToLayer("Level")) | layerMaskTarget);

                    if (hit.collider != null &&
                        layerMaskTarget == (layerMaskTarget | (1 << hit.collider.gameObject.layer)))
                    {
                        LookEvent.Invoke(direction);
                        OnMoveEvent.Invoke(Vector2.zero);
                        IsAttacking = true;
                    }
                    else
                    {
                        OnMoveEvent.Invoke(direction);
                    }
                }
                else
                {
                    OnMoveEvent.Invoke(direction);
                }
            }
            else
            {
                OnMoveEvent.Invoke(Vector2.zero);
            }
        }
    }
}