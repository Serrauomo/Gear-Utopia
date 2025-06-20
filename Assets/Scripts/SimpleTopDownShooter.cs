using TopDownCharacter2D.Attacks;
using TopDownCharacter2D.Attacks.Range;
using TopDownCharacter2D.Controllers;
using UnityEngine;

namespace TopDownCharacter2D
{
    /// <summary>
    ///     Simplified shooting class for enemies that shoot from their center without rotating weapons
    /// </summary>
    [RequireComponent(typeof(TopDownCharacterController))]
    public class TopDownSimpleShooting : MonoBehaviour
    {
        [Header("Parameters")] 
        [SerializeField] [Range(0.0f, 2f)] [Tooltip("The strength of the recoil after a shot")]
        private float recoilStrength = 0.5f; // Ridotto per nemici

        [SerializeField] [Tooltip("Define if the recoil is proportional to the size of the projectile")]
        private bool projectileSizeModifyRecoil = false; // Disabilitato per semplicità

        [SerializeField] [Tooltip("Offset from center where projectiles spawn")]
        private Vector2 projectileOffset = Vector2.zero; // Offset dal centro se necessario

        private Vector2 _aimDirection = Vector2.right;
        private ProjectileManager projectileManager;
        private TopDownCharacterController _controller;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _controller = GetComponent<TopDownCharacterController>();
        }

        private void Start()
        {
            projectileManager = ProjectileManager.instance;
            _controller.OnAttackEvent.AddListener(OnShoot);
            _controller.LookEvent.AddListener(OnAim);
        }

        /// <summary>
        ///     To call when the aim changes direction
        /// </summary>
        /// <param name="newAimDirection"> The new aim direction </param>
        public void OnAim(Vector2 newAimDirection)
        {
            _aimDirection = newAimDirection.normalized;
        }

        /// <summary>
        ///     To call when the enemy tries to shoot
        /// </summary>
        /// <param name="attackConfig"> The stats of the projectile to shoot </param>
        public void OnShoot(AttackConfig attackConfig)
        {
            RangedAttackConfig rangedAttackConfig = (RangedAttackConfig) attackConfig;
            float projectilesAngleSpace = rangedAttackConfig.multipleProjectilesAngle;
            float minAngle = -(rangedAttackConfig.numberOfProjectilesPerShot / 2f) * projectilesAngleSpace +
                             0.5f * rangedAttackConfig.multipleProjectilesAngle;

            for (int i = 0; i < rangedAttackConfig.numberOfProjectilesPerShot; i++)
            {
                float angle = minAngle + projectilesAngleSpace * i;
                float randomSpread = Random.Range(-rangedAttackConfig.spread, rangedAttackConfig.spread);
                angle += randomSpread;
                CreateProjectile(rangedAttackConfig, angle);
            }

            if (_rb != null && recoilStrength > 0)
            {
                AddRecoil(rangedAttackConfig);
            }
        }

        /// <summary>
        ///     Creates a projectile from the enemy's center (plus offset)
        /// </summary>
        /// <param name="rangedAttackConfig"> the configuration of the projectile to shoot </param>
        /// <param name="angle"> Modification to the direction of the shot</param>
        private void CreateProjectile(RangedAttackConfig rangedAttackConfig, float angle)
        {
            // Calcola la posizione di spawn: centro del nemico + offset ruotato nella direzione di mira
            Vector2 rotatedOffset = RotateVector2(projectileOffset, Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg);
            Vector3 spawnPosition = transform.position + (Vector3)rotatedOffset;
            
            Vector2 shootDirection = RotateVector2(_aimDirection, angle);
            
            projectileManager.ShootBullet(spawnPosition, shootDirection, rangedAttackConfig);
        }

        /// <summary>
        ///     Rotates a Vector2 by a set amount of degrees
        /// </summary>
        /// <param name="v"> The vector to rotate </param>
        /// <param name="degrees"> The angle in degree </param>
        /// <returns></returns>
        private static Vector2 RotateVector2(Vector2 v, float degrees)
        {
            return Quaternion.Euler(0, 0, degrees) * v;
        }

        /// <summary>
        ///     Adds a light recoil for the enemy
        /// </summary>
        /// <param name="rangedAttackConfig"> the configuration of the projectile shot </param>
        private void AddRecoil(RangedAttackConfig rangedAttackConfig)
        {
            if (projectileSizeModifyRecoil)
            {
                _rb.AddForce(-(_aimDirection * (rangedAttackConfig.size * recoilStrength * 100f)), ForceMode2D.Impulse);
            }
            else
            {
                _rb.AddForce(-(_aimDirection * (recoilStrength * 100f)), ForceMode2D.Impulse);
            }
        }
    }
}