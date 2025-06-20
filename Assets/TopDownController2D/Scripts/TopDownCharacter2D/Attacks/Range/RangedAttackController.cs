using TopDownCharacter2D.Health;
using UnityEngine;

namespace TopDownCharacter2D.Attacks.Range
{
    /// <summary>
    ///     This script handles the logic of a single bullet
    /// </summary>
    public class RangedAttackController : MonoBehaviour
    {
        [Tooltip("The layer of the walls of the level")] [SerializeField]
        private LayerMask levelCollisionLayer;

        private RangedAttackConfig _config;
        private float _currentDuration;
        private Vector2 _direction;
        private ParticleSystem _impactParticleSystem;
        private bool _isReady;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private TrailRenderer _trail;
        private ProjectileManager _projectileManager;

        private bool fxOnDestroy = true;

        private bool DestroyOnHit { get; set; } = true;

        public ref Vector2 Direction => ref _direction;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            _trail = GetComponent<TrailRenderer>();
        }

        private void Update()
        {
            if (!_isReady)
            {
                return;
            }

            _currentDuration += Time.deltaTime;

            if (_currentDuration > _config.duration)
            {
                DestroyProjectile(transform.position, false);
            }

            _rb.linearVelocity = _direction * _config.speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (levelCollisionLayer.value == (levelCollisionLayer.value | (1 << other.gameObject.layer)))
            {
                if (DestroyOnHit)
                {
                    DestroyProjectile(other.ClosestPoint(transform.position) - _direction * .2f, fxOnDestroy);
                }
            }
            else if (_config.target.value == (_config.target.value | (1 << other.gameObject.layer)))
            {
                HealthSystem health = other.gameObject.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.ChangeHealth(-_config.power);
                    TopDownKnockBack knockBack = other.gameObject.GetComponent<TopDownKnockBack>();
                    if (knockBack != null)
                    {
                        knockBack.ApplyKnockBack(transform);
                    }
                }

                if (DestroyOnHit)
                {
                    DestroyProjectile(other.ClosestPoint(transform.position), fxOnDestroy);
                }
            }
        }

        /// <summary>
        ///     Initializes the ranged attack with the given configuration
        /// </summary>
        /// <param name="direction"> The direction of the attack </param>
        /// <param name="config"> The parameters of the ranged attack</param>
        /// <param name="projectileManager"></param>
        public void InitializeAttack(Vector2 direction, RangedAttackConfig config, ProjectileManager projectileManager)
        {
            _projectileManager = projectileManager;
            _config = config;
            _direction = direction;
            UpdateProjectileSprite();
            _trail.Clear();
            _currentDuration = 0f;
            _spriteRenderer.color = config.projectileColor;

            _isReady = true;
        }

        /// <summary>
        ///     Changes the sprite of the projectile according to its size
        /// </summary>
        private void UpdateProjectileSprite()
        {
            transform.localScale = Vector3.one * _config.size;
        }

        /// <summary>
        ///     Destroys the projectile
        /// </summary>
        /// <param name="pos">The position where to create the particles</param>
        /// <param name="createFx">Whether to create particles or not</param>
        private void DestroyProjectile(Vector2 pos, bool createFx)
        {
            if (createFx)
            {
                _projectileManager.CreateImpactParticlesAtPosition(pos, _config);
            }
            gameObject.SetActive(false);
        }
    }
}