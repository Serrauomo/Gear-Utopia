using UnityEngine;

namespace TopDownCharacter2D
{
    /// <summary>
    ///     This class handles the logic behind a knockback impulse
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownKnockBack : MonoBehaviour
    {
        [SerializeField] private float knockBackPower;
        [SerializeField] private float timeBetweenKnockBack = .1f;

        private Rigidbody2D _rb;
        private float _timeSinceLastKnockBack = float.MaxValue;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_timeSinceLastKnockBack < timeBetweenKnockBack)
            {
                _timeSinceLastKnockBack += Time.deltaTime;
            }
        }

        /// <summary>
        ///     Applies the knockback to the entity by pushing it in the opposite direction
        /// </summary>
        /// <param name="other"> The entity who caused the knockback </param>
        public void ApplyKnockBack(Transform other)
        {
            if (_timeSinceLastKnockBack < timeBetweenKnockBack)
            {
                return;
            }

            Vector2 direction = -(other.position - transform.position).normalized;

            _rb.AddForce(direction * (knockBackPower * 100f), ForceMode2D.Impulse);
        }
    }
}