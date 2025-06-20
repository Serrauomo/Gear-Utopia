using TopDownCharacter2D.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace TopDownCharacter2D.Health
{
    /// <summary>
    ///     Handles the health of an entity with additional safety checks
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Tooltip("The delay between two health changes in seconds")]
        [SerializeField] private float healthChangeDelay = .5f;

        [SerializeField] private UnityEvent onDamage;
        [SerializeField] private UnityEvent onHeal;
        [SerializeField] private UnityEvent onDeath;
        [SerializeField] private UnityEvent onInvincibilityEnd;

        private CharacterStatsHandler _statsHandler;
        private float _timeSinceLastChange = float.MaxValue;
        private bool _isDead = false;

        public UnityEvent OnDamage => onDamage;
        public UnityEvent OnHeal => onHeal;
        public UnityEvent OnDeath => onDeath;
        public UnityEvent OnInvincibilityEnd => onInvincibilityEnd;

        public float CurrentHealth { get; private set; }
        public bool IsDead => _isDead;

        public GameManagerScript gameManager;

        public float MaxHealth => _statsHandler != null ? _statsHandler.CurrentStats.maxHealth : 100f;

        private void Awake()
        {
            _statsHandler = GetComponent<CharacterStatsHandler>();
        }

        private void Start()
        {
            if (_statsHandler != null)
            {
                CurrentHealth = _statsHandler.CurrentStats.maxHealth;
            }
            else
            {
                CurrentHealth = 100f; // Fallback value
                Debug.LogWarning("CharacterStatsHandler not found, using default health value");
            }
        }

        private void Update()
        {
            if (_timeSinceLastChange < healthChangeDelay)
            {
                _timeSinceLastChange += Time.deltaTime;
                if (_timeSinceLastChange >= healthChangeDelay)
                {
                    try
                    {
                        onInvincibilityEnd?.Invoke();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Error invoking invincibility end event: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        ///     Modifies the health of the entity
        /// </summary>
        /// <param name="change"> The amount of health to add</param>
        /// <returns></returns>
        public bool ChangeHealth(float change)
        {
            // If already dead, do not process further changes
            if (_isDead)
            {
                return false;
            }

            if (change == 0 || _timeSinceLastChange < healthChangeDelay)
            {
                return false;
            }

            _timeSinceLastChange = 0f;
            float previousHealth = CurrentHealth;
            CurrentHealth += change;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            try
            {
                if (change > 0)
                {
                    onHeal?.Invoke();
                }
                else
                {
                    onDamage?.Invoke();
                }
            }
            catch (System.Exception)
            {
                // Silently ignore event invocation errors
            }

            // Death handling with additional checks
            if (CurrentHealth <= 0f && !_isDead)
            {
                Debug.Log("Character is dying...");
                HandleDeath();
            }

            return true;
        }

        /// <summary>
        /// Handles the character's death safely
        /// </summary>
        private void HandleDeath()
        {
            if (_isDead) return;

            _isDead = true;

            if (!CompareTag("Player"))
            {
                ScoreManager.instance.AddPoint();

                // Dai XP al player
                if (ExpManager.Instance != null)
                    ExpManager.Instance.GainExperience(3); // Cambia 3 con il valore XP che vuoi
            }

            try
            {
                onDeath?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error invoking death event: {e.Message}");
            }

            // Game manager handling
            if (gameManager != null)
            {
                try
                {
                    gameManager.gameOver();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error calling game over: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Forces the character's death (for debugging)
        /// </summary>
        [ContextMenu("Force Death")]
        public void ForceDeath()
        {
            CurrentHealth = 0f;
            HandleDeath();
        }
    }
}