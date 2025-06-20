using TopDownCharacter2D.Health;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TopDownCharacter2D.UI
{
    /// <summary>
    ///     Updates the health gauge of an entity
    /// </summary>
    public class EntityHealthGaugeUpdater : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private GameObject entityObject;

        [SerializeField] private UnityEvent onHealthUpdate;

        private HealthSystem _entityHealth;

        private void Awake()
        {
            _entityHealth = entityObject.GetComponent<HealthSystem>();
        }

        private void Start()
        {
            _entityHealth.OnDamage.AddListener(UpdateHealth);
            _entityHealth.OnHeal.AddListener(UpdateHealth);
        }

        /// <summary>
        ///     Updates the health bar slider's value
        /// </summary>
        private void UpdateHealth()
        {
            healthSlider.value = _entityHealth.CurrentHealth / _entityHealth.MaxHealth;
            onHealthUpdate.Invoke();
        }
    }
}