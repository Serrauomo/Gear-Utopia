using TopDownCharacter2D.Attacks;
using TopDownCharacter2D.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace TopDownCharacter2D.Controllers
{
    /// <summary>
    ///     A basic controller for a character
    /// </summary>
    [RequireComponent(typeof(CharacterStatsHandler))]
    public abstract class TopDownCharacterController : MonoBehaviour
    {
        private float _timeSinceLastAttack = float.MaxValue;

        protected bool IsAttacking { get; set; }
        protected CharacterStatsHandler Stats { get; private set; }

        protected virtual void Awake()
        {
            Stats = GetComponent<CharacterStatsHandler>();
        }

        protected virtual void Update()
        {
            HandleAttackDelay();
        }

        /// <summary>
        ///     Only trigger a attack event when the attack delay is over
        /// </summary>
        private void HandleAttackDelay()
        {
            if (Stats.CurrentStats.attackConfig == null)
            {
                return;
            }

            if (_timeSinceLastAttack <= Stats.CurrentStats.attackConfig.delay)
            {
                _timeSinceLastAttack += Time.deltaTime;
            }

            if (IsAttacking && _timeSinceLastAttack > Stats.CurrentStats.attackConfig.delay)
            {
                _timeSinceLastAttack = 0f;
                onAttackEvent.Invoke(Stats.CurrentStats.attackConfig);
            }
        }

        #region Events

        private readonly MoveEvent onMoveEvent = new MoveEvent();
        private readonly AttackEvent onAttackEvent = new AttackEvent();
        private readonly LookEvent onLookEvent = new LookEvent();

        public UnityEvent<Vector2> OnMoveEvent => onMoveEvent;
        public UnityEvent<AttackConfig> OnAttackEvent => onAttackEvent;
        public UnityEvent<Vector2> LookEvent => onLookEvent;

        #endregion
    }
}