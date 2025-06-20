using UnityEngine;

namespace TopDownCharacter2D.Controllers
{
    /// <summary>
    ///     A pattern-based enemy AI that moves in a predefined pattern (horizontal + vertical oscillation)
    ///     without following the player. Deals damage via a ChangeHealthOnTouch component.
    /// </summary>
    public class TopDownPatternEnemyController : TopDownEnemyController
    {
        [Header("Movement Pattern Settings")]
        [SerializeField] private MovementDirection horizontalDirection = MovementDirection.Right;
        [SerializeField] private float horizontalSpeed = 3f;
        
        [Header("Vertical Oscillation Settings")]
        [SerializeField] private bool enableVerticalOscillation = true;
        [SerializeField] private float verticalAmplitude = 2f; // Amplitude of vertical movement
        [SerializeField] private float verticalFrequency = 1f; // Speed of vertical oscillation
        
        [Header("Boundary Settings")]
        [SerializeField] private bool useScreenBounds = true;
        [SerializeField] private float customLeftBound = -10f;
        [SerializeField] private float customRightBound = 10f;
        [SerializeField] private float boundaryOffset = 1f; // Offset from screen edges
        
        [Header("Visual Settings")]
        [SerializeField] private Transform visualTransform; // Reference to the sprite's transform
        [SerializeField] private bool flipSpriteOnDirectionChange = true;
        
        private Camera _mainCamera;
        private Vector3 _startPosition;
        private float _verticalTimer = 0f;
        private bool _movingRight = true;
        private float _leftBoundary;
        private float _rightBoundary;
        
        public enum MovementDirection
        {
            Right,
            Left
        }

        protected override void Awake()
        {
            base.Awake();
            
            // Find the main camera
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                _mainCamera = FindObjectOfType<Camera>();
            }
            
            // If not manually assigned, automatically find the visual transform
            if (visualTransform == null)
            {
                Transform mainSprite = transform.Find("MainSprite");
                if (mainSprite != null)
                {
                    visualTransform = mainSprite;
                }
                else
                {
                    visualTransform = transform.childCount > 0 ? transform.GetChild(0) : transform;
                }
            }
            
            // Save the starting position
            _startPosition = transform.position;
            
            // Set the initial direction
            _movingRight = (horizontalDirection == MovementDirection.Right);
            
            // Calculate boundaries
            CalculateBoundaries();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            
            // Calculate horizontal movement
            Vector2 horizontalMovement = CalculateHorizontalMovement();
            
            // Calculate vertical movement (oscillation)
            Vector2 verticalMovement = CalculateVerticalMovement();
            
            // Combine movements
            Vector2 totalMovement = horizontalMovement + verticalMovement;
            
            // Apply movement
            OnMoveEvent.Invoke(totalMovement);
            
            // Update sprite flip if necessary
            UpdateSpriteFlip();
        }

        /// <summary>
        /// Calculates horizontal movement with boundary control
        /// </summary>
        private Vector2 CalculateHorizontalMovement()
        {
            // Always moves in the chosen direction, without reversing at the edges
            float dir = (horizontalDirection == MovementDirection.Right) ? 1f : -1f;
            return new Vector2(dir * horizontalSpeed, 0f);
        }

        /// <summary>
        /// Calculates vertical oscillatory movement
        /// </summary>
        private Vector2 CalculateVerticalMovement()
        {
            if (!enableVerticalOscillation)
            {
                return Vector2.zero;
            }
            
            // Update the timer
            _verticalTimer += Time.fixedDeltaTime * verticalFrequency;
            
            // Calculate vertical velocity using the derivative of sine
            float verticalVelocity = Mathf.Cos(_verticalTimer) * verticalAmplitude * verticalFrequency;
            
            return new Vector2(0f, verticalVelocity);
        }

        /// <summary>
        /// Updates the sprite flip based on direction
        /// </summary>
        private void UpdateSpriteFlip()
        {
            if (!flipSpriteOnDirectionChange || visualTransform == null)
                return;
            
            Vector3 scale = visualTransform.localScale;
            
            if (_movingRight && scale.x < 0)
            {
                scale.x = Mathf.Abs(scale.x);
                visualTransform.localScale = scale;
            }
            else if (!_movingRight && scale.x > 0)
            {
                scale.x = -Mathf.Abs(scale.x);
                visualTransform.localScale = scale;
            }
        }

        /// <summary>
        /// Calculates the movement boundaries
        /// </summary>
        private void CalculateBoundaries()
        {
            if (useScreenBounds && _mainCamera != null)
            {
                // Use screen boundaries
                Vector3 screenBounds = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, _mainCamera.transform.position.z));
                _leftBoundary = -screenBounds.x + boundaryOffset;
                _rightBoundary = screenBounds.x - boundaryOffset;
            }
            else
            {
                // Use custom boundaries
                _leftBoundary = customLeftBound;
                _rightBoundary = customRightBound;
            }
        }

        /// <summary>
        /// Resets the enemy's position and pattern
        /// </summary>
        public void ResetPattern()
        {
            transform.position = _startPosition;
            _movingRight = (horizontalDirection == MovementDirection.Right);
            _verticalTimer = 0f;
        }

        /// <summary>
        /// Changes the horizontal movement direction
        /// </summary>
        public void ChangeHorizontalDirection()
        {
            _movingRight = !_movingRight;
        }

        /// <summary>
        /// Sets the pattern parameters at runtime
        /// </summary>
        public void SetPatternParameters(float hSpeed, float vAmplitude, float vFrequency)
        {
            horizontalSpeed = hSpeed;
            verticalAmplitude = vAmplitude;
            verticalFrequency = vFrequency;
        }

        /// <summary>
        /// Sets custom boundaries
        /// </summary>
        public void SetCustomBounds(float left, float right)
        {
            useScreenBounds = false;
            customLeftBound = left;
            customRightBound = right;
            CalculateBoundaries();
        }

        // Debug methods - display boundaries in the editor
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(new Vector3(_leftBoundary, transform.position.y - 5f, 0f), 
                               new Vector3(_leftBoundary, transform.position.y + 5f, 0f));
                Gizmos.DrawLine(new Vector3(_rightBoundary, transform.position.y - 5f, 0f), 
                               new Vector3(_rightBoundary, transform.position.y + 5f, 0f));
                
                if (enableVerticalOscillation)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(_startPosition, verticalAmplitude);
                }
            }
        }
    }
}