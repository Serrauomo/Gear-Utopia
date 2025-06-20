using TopDownCharacter2D.Health;
using TopDownCharacter2D.Controllers;
using UnityEngine;
using UnityEngine.InputSystem; // aggiungi questo using

namespace TopDownController2D.Scripts.TopDownCharacter2D.Animations
{
    public class SampleCharacterAnimation : TopDownAnimations
    {
        private static readonly int IsHurt = Animator.StringToHash("IsHurt");
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");

        private HealthSystem _healthSystem;
        private TopDownCharacterController _controller;

        protected override void Awake()
        {
            base.Awake();
            _healthSystem = GetComponent<HealthSystem>();
            _controller = GetComponent<TopDownCharacterController>();
        }

        protected void Start()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDamage.AddListener(Hurt);
                _healthSystem.OnInvincibilityEnd.AddListener(InvincibilityEnd);
            }
            if (_controller != null)
            {
                _controller.OnMoveEvent.AddListener(OnMove);
            }
        }

        /// <summary>
        ///     To call when the character takes damage
        /// </summary>
        private void Hurt()
        {
            animator.SetBool(IsHurt, true);
        }

        /// <summary>
        ///     To call when the character ends its invincibility time
        /// </summary>
        public void InvincibilityEnd()
        {
            animator.SetBool(IsHurt, false);
        }

        /// <summary>
        ///     Updates the IsWalking state based on movement
        /// </summary>
        public void OnMove(InputValue value)
        {
            Vector2 direction = value.Get<Vector2>();
            OnMove(direction);
        }

        public void OnMove(Vector2 direction)
        {
            animator.SetBool(IsWalking, direction.sqrMagnitude > 0.01f);
        }
    }
}