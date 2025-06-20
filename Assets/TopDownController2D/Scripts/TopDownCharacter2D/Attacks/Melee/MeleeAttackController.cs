using TopDownCharacter2D.Health;
using UnityEngine;

namespace TopDownCharacter2D.Attacks.Melee
{
    /// <summary>
    ///     Contains the logic for a single melee attack
    /// </summary>
    public class MeleeAttackController : MonoBehaviour
    {
        private MeleeAttackConfig attackConfig;
        private Vector3 _endPosition;
        private Quaternion _endRotation;
        private bool _isReady;
        private Vector3 _startPosition;

        private Quaternion _startRotation;
        private float _timeActive;

        private Transform _transform;

        private void Update()
        {
            if (!_isReady)
            {
                return;
            }

            _timeActive += Time.deltaTime;

            //  Destroy the attack after the time of the attack speed
            if (_timeActive > attackConfig.speed)
            {
                DestroyAttack();
            }

            // Apply the swing and thrust transformations
            _transform.localRotation = Quaternion.Lerp(_startRotation, _endRotation,
                attackConfig.swingCurve.Evaluate(_timeActive / attackConfig.speed));
            _transform.localPosition = _transform.localRotation * Vector3.Lerp(_startPosition, _endPosition,
                attackConfig.thrustCurve.Evaluate(_timeActive / attackConfig.speed));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (attackConfig.target.value == (attackConfig.target.value | (1 << other.gameObject.layer)))
            {
                HealthSystem health = other.gameObject.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.ChangeHealth(-attackConfig.power);
                    TopDownKnockBack knockBack = other.gameObject.GetComponent<TopDownKnockBack>();
                    if (knockBack != null)
                    {
                        knockBack.ApplyKnockBack(transform);
                    }
                }
            }
        }

        /// <summary>
        ///     Initializes the properties of the attack
        /// </summary>
        /// <param name="attackConfig"> The configuration of the attack </param>
        public void InitializeAttack(MeleeAttackConfig attackConfig)
        {
            _transform = transform;
            this.attackConfig = attackConfig;

            ComputeSwingRotations();
            ComputeThrustPositions();
            ScaleAttack();

            _transform.localRotation = _startRotation;
            _transform.localPosition = _startPosition;

            _timeActive = 0f;
            _isReady = true;
        }

        /// <summary>
        ///     Computes the start and end swing rotation of the melee attack based on the swingAngle of the configuration
        /// </summary>
        private void ComputeSwingRotations()
        {
            Quaternion rotation = _transform.rotation;
            _startRotation = rotation * Quaternion.Euler(0, 0, -attackConfig.swingAngle);
            _endRotation = rotation * Quaternion.Euler(0, 0, attackConfig.swingAngle);
        }

        /// <summary>
        ///     Computes the start and end thrust position of the melee attack based on the thrustDistance of the configuration
        /// </summary>
        private void ComputeThrustPositions()
        {
            Vector3 position = _transform.localPosition;
            _startPosition = position;
            _endPosition = position + new Vector3(attackConfig.thrustDistance, 0, 0);
        }

        /// <summary>
        ///     Changes the scale of the attack to match the size in the configuration
        /// </summary>
        private void ScaleAttack()
        {
            transform.localScale = new Vector3(attackConfig.size, attackConfig.size, attackConfig.size);
        }

        /// <summary>
        ///     Destroys the attack
        /// </summary>
        private void DestroyAttack()
        {
            Destroy(gameObject);
        }
    }
}