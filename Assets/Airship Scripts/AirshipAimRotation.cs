using System.Collections.Generic;
using TopDownCharacter2D.Controllers;
using UnityEngine;

namespace TopDownCharacter2D
{
    /// <summary>
    /// Sistema di rotazione semplificato per il dirigibile - solo flip orizzontale
    /// </summary>
    [RequireComponent(typeof(TopDownCharacterController))]
    public class AirshipAimRotation : MonoBehaviour
    {
        [SerializeField] [Tooltip("I renderer principali del dirigibile")]
        private List<SpriteRenderer> airshipRenderers;
        
        [SerializeField] [Tooltip("Ruota tutto il dirigibile invece di solo flipparlo?")]
        private bool rotateEntireAirship = false;
        
        [SerializeField] [Tooltip("Velocità di rotazione se rotateEntireAirship è true")]
        private float rotationSpeed = 2f;
        
        private TopDownCharacterController _controller;
        private Vector2 _lastAimDirection;

        private void Awake()
        {
            _controller = GetComponent<TopDownCharacterController>();
        }

        private void Start()
        {
            _controller.LookEvent.AddListener(OnAim);
        }

        public void OnAim(Vector2 newAimDirection)
        {
            _lastAimDirection = newAimDirection;
            
            if (rotateEntireAirship)
            {
                RotateAirship(newAimDirection);
            }
            else
            {
                FlipAirship(newAimDirection);
            }
        }
        
        private void FlipAirship(Vector2 direction)
        {
            // Semplice flip orizzontale basato sulla direzione
            bool shouldFlip = direction.x < 0;
            
            foreach (SpriteRenderer renderer in airshipRenderers)
            {
                if (renderer != null)
                {
                    renderer.flipX = shouldFlip;
                }
            }
        }
        
        private void RotateAirship(Vector2 direction)
        {
            // Rotazione graduale verso la direzione target
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Interpola la rotazione per un movimento più fluido
            float currentAngle = transform.eulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
    }
}