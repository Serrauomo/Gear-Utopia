using TopDownCharacter2D.Controllers;
using TopDownCharacter2D.Stats;
using UnityEngine;

namespace TopDownCharacter2D
{
    /// <summary>
    ///     This class contains the logic for movement in a 2D environment with a top down view
    /// </summary>
    [RequireComponent(typeof(CharacterStatsHandler))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(TopDownCharacterController))]
    public class TopDownMovement : MonoBehaviour
    {
        private TopDownCharacterController _controller;

        private Vector2 _movementDirection = Vector2.zero;
        private Rigidbody2D _rb;
        private CharacterStatsHandler _stats;

        private void Awake()
        {
            _controller = GetComponent<TopDownCharacterController>();
            _stats = GetComponent<CharacterStatsHandler>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _controller.OnMoveEvent.AddListener(Move);
        }

        private void FixedUpdate()
        {
            ApplyMovement(_movementDirection);
        }

        /// <summary>
        ///     Changes the current direction of the movement
        /// </summary>
        /// <param name="direction"></param>
        private void Move(Vector2 direction)
        {
            _movementDirection = direction;
        }

        /// <summary>
        ///     Used to apply a given movement vector to the rigidbody of the character
        /// </summary>
        /// <param name="direction"> The direction in which to move the rigidbody </param>
        private void ApplyMovement(Vector2 direction)
        {
            _rb.linearVelocity += direction * _stats.CurrentStats.speed;
        }
    }
}