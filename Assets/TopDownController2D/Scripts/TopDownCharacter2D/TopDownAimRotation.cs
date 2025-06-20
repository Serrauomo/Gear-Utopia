using System.Collections.Generic;
using TopDownCharacter2D.Controllers;
using UnityEngine;

namespace TopDownCharacter2D
{
    /// <summary>
    ///     Handles the logic behind the aim rotation
    /// </summary>
    [RequireComponent(typeof(TopDownCharacterController))]
    public class TopDownAimRotation : MonoBehaviour
    {
        [SerializeField] [Tooltip("The renderer of the arm used to aim")]
        private SpriteRenderer armRenderer;

        [SerializeField] [Tooltip("The main renderer of the character")]
        private List<SpriteRenderer> characterRenderers;

        [SerializeField] [Tooltip("The origin point of the arm to aim with")]
        private Transform armPivot;

        private TopDownCharacterController _controller;

        private void Awake()
        {
            _controller = GetComponent<TopDownCharacterController>();
        }

        private void Start()
        {
            _controller.LookEvent.AddListener(OnAim);
        }

        /// <summary>
        ///     To call when the aim direction changes
        /// </summary>
        /// <param name="newAimDirection"> The new aim direction </param>
        public void OnAim(Vector2 newAimDirection)
        {
            RotateArm(newAimDirection);
        }

        /// <summary>
        ///     Rotates the arm around the arm's pivot point, and flips the player's sprite if needed
        /// </summary>
        /// <param name="direction"> The new aim direction </param>
        private void RotateArm(Vector2 direction)
        {
            float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bool shouldFlipCharacter = Mathf.Abs(rotZ) > 90f;

            // Flip the character
            foreach (SpriteRenderer charRenderer in characterRenderers)
            {
                charRenderer.flipX = shouldFlipCharacter;
            }

            // Move ONLY the armPivot, do NOT touch rotation and flip for now
            Vector3 currentPos = armPivot.localPosition;
            if (shouldFlipCharacter)
            {
                armPivot.localPosition = new Vector3(-Mathf.Abs(currentPos.x), currentPos.y, currentPos.z);
            }
            else
            {
                armPivot.localPosition = new Vector3(Mathf.Abs(currentPos.x), currentPos.y, currentPos.z);
            }

            // Keep EXACTLY the original logic for rotation and flip
            armRenderer.flipY = Mathf.Abs(rotZ) > 90f;
            armPivot.rotation = Quaternion.Euler(0, 0, rotZ);
        }
    }
}