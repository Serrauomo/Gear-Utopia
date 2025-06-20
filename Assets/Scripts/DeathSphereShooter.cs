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
    public class DeathSphereShooter : MonoBehaviour
    {
        [Header("Parameters")] 
        [SerializeField] [Range(0.0f, 2f)] [Tooltip("The strength of the recoil after a shot")]
        private float recoilStrength = 0.5f;

        [SerializeField] [Tooltip("Define if the recoil is proportional to the size of the projectile")]
        private bool projectileSizeModifyRecoil = false;

        [SerializeField] [Tooltip("Offset from center where projectiles spawn")]
        private Vector2 projectileOffset = Vector2.zero;

        [Header("Animation")]
        [SerializeField] [Tooltip("Reference to the Animator component")]
        private Animator animator;

        private Vector2 _aimDirection = Vector2.right;
        private ProjectileManager projectileManager;
        private TopDownCharacterController _controller;
        private Rigidbody2D _rb;
        
        // Stati per il controllo delle animazioni
        private bool _isOpen = false;
        private bool _canShoot = false; // Diventa true quando l'animazione di apertura finisce
        private bool _isCurrentlyMoving = false;
        private RangedAttackConfig _pendingAttackConfig; // Salva l'attacco in attesa

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _controller = GetComponent<TopDownCharacterController>();
            
            // Se non è assegnato nell'inspector, prova a trovarlo
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void Start()
        {
            projectileManager = ProjectileManager.instance;
            _controller.OnAttackEvent.AddListener(OnShoot);
            _controller.LookEvent.AddListener(OnAim);
            
            // Ascolta anche gli eventi di movimento per chiudere la palla
            if (_controller != null)
            {
                // Assumendo che il controller abbia un modo per rilevare il movimento
                // Potresti dover adattare questo in base al tuo TopDownCharacterController
            }
        }

        private void Update()
        {
            // Controlla se il nemico si sta muovendo
            CheckMovementAndCloseIfNeeded();
        }

        /// <summary>
        ///     Controlla se il nemico si sta muovendo e gestisce le transizioni di stato
        /// </summary>
        private void CheckMovementAndCloseIfNeeded()
        {
            if (_rb != null && animator != null)
            {
                bool isMoving = _rb.linearVelocity.magnitude > 0.1f;
                
                // Se il movimento è cambiato
                if (isMoving != _isCurrentlyMoving)
                {
                    _isCurrentlyMoving = isMoving;
                    
                    if (isMoving)
                    {
                        // Ha iniziato a muoversi - chiudi la palla
                        _isOpen = false;
                        _canShoot = false;
                        _pendingAttackConfig = null;
                        animator.SetBool("isMoving", true);
                    }
                    else
                    {
                        // Si è fermato - vai allo stato closed (fermo)
                        animator.SetBool("isMoving", false);
                        animator.SetTrigger("stopMoving");
                    }
                }
            }
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
            
            // Può sparare solo se non si sta muovendo
            if (_isCurrentlyMoving)
            {
                return; // Ignora il comando di sparo se si sta muovendo
            }
            
            if (!_isOpen)
            {
                // Se la palla è chiusa, inizia l'animazione di apertura
                _pendingAttackConfig = rangedAttackConfig;
                OpenBall();
            }
            else if (_canShoot)
            {
                // Se la palla è già aperta e può sparare, spara immediatamente
                ExecuteShoot(rangedAttackConfig);
            }
            else
            {
                // Se sta ancora aprendo, salva l'attacco per dopo
                _pendingAttackConfig = rangedAttackConfig;
            }
        }

        /// <summary>
        ///     Inizia l'animazione di apertura della palla
        /// </summary>
        private void OpenBall()
        {
            if (animator != null && !_isOpen)
            {
                _isOpen = true;
                _canShoot = false;
                animator.SetTrigger("isOpening");
            }
        }

        /// <summary>
        ///     Chiude la palla quando il nemico si muove - ora gestito da CheckMovementAndCloseIfNeeded
        /// </summary>
        private void CloseBall()
        {
            // Logica spostata in CheckMovementAndCloseIfNeeded
        }

        /// <summary>
        ///     Resetta il parametro di movimento nell'animator - non più necessario
        /// </summary>
        private void ResetMovingParameter()
        {
            // Metodo mantenuto per compatibilità ma non più utilizzato
        }

        /// <summary>
        ///     Chiamato dall'Animation Event quando l'animazione di apertura finisce
        /// </summary>
        public void OnOpeningAnimationComplete()
        {
            _canShoot = true;
            
            // Se c'è un attacco in attesa, eseguilo
            if (_pendingAttackConfig != null)
            {
                ExecuteShoot(_pendingAttackConfig);
                _pendingAttackConfig = null;
            }
        }

        /// <summary>
        ///     Esegue effettivamente lo sparo
        /// </summary>
        /// <param name="rangedAttackConfig">Configurazione dell'attacco</param>
        private void ExecuteShoot(RangedAttackConfig rangedAttackConfig)
        {
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

        /// <summary>
        ///     Metodo pubblico per forzare la chiusura della palla (utile per debug o eventi esterni)
        /// </summary>
        public void ForceCloseBall()
        {
            if (animator != null)
            {
                _isOpen = false;
                _canShoot = false;
                _pendingAttackConfig = null;
                animator.SetBool("isMoving", true);
            }
        }

        /// <summary>
        ///     Proprietà per controllare lo stato dall'esterno
        /// </summary>
        public bool IsOpen => _isOpen;
        public bool CanShoot => _canShoot;
        public bool IsMoving => _isCurrentlyMoving;
    }
}