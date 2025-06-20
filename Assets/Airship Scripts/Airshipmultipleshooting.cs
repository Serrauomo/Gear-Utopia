using TopDownCharacter2D.Attacks;
using TopDownCharacter2D.Attacks.Range;
using TopDownCharacter2D.Controllers;
using UnityEngine;

namespace TopDownCharacter2D
{
    /// <summary>
    /// Sistema di sparo per dirigibile con multipli punti di spawn
    /// </summary>
    [RequireComponent(typeof(TopDownCharacterController))]
    public class AirshipMultiPointShooting : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] [Tooltip("Tutti i punti da cui sparare i proiettili")]
        private Transform[] projectileSpawnPoints;
        
        [Header("Shooting Pattern")]
        [SerializeField] [Tooltip("Spara da tutti i punti contemporaneamente?")]
        private bool shootFromAllPoints = true;
        
        [SerializeField] [Tooltip("Se false, spara da punti casuali")]
        private bool shootInSequence = false;
        
        [SerializeField] [Tooltip("Quanti punti usare se non tutti (0 = tutti)")]
        private int maxPointsPerShot = 0;
        
        [Header("Recoil")]
        [SerializeField] [Range(0.0f, 1f)] [Tooltip("Ridotto per dirigibile pesante")]
        private float recoilStrength = 0.1f;
        
        private Vector2 _aimDirection = Vector2.right;
        private ProjectileManager projectileManager;
        private TopDownCharacterController _controller;
        private Rigidbody2D _rb;
        private int _currentSpawnIndex = 0; // Per il firing in sequenza
        
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
            
            // Validazione spawn points
            if (projectileSpawnPoints == null || projectileSpawnPoints.Length == 0)
            {
                Debug.LogError("Nessun spawn point assegnato al dirigibile!");
            }
        }

        public void OnAim(Vector2 newAimDirection)
        {
            _aimDirection = newAimDirection;
        }

        public void OnShoot(AttackConfig attackConfig)
        {
            RangedAttackConfig rangedAttackConfig = (RangedAttackConfig)attackConfig;
            
            if (projectileSpawnPoints == null || projectileSpawnPoints.Length == 0)
                return;
            
            // Determina da quali punti sparare
            Transform[] activeSpawnPoints = GetActiveSpawnPoints();
            
            // Spara da ogni punto attivo
            foreach (Transform spawnPoint in activeSpawnPoints)
            {
                ShootFromPoint(spawnPoint, rangedAttackConfig);
            }
            
            // Applica recoil ridotto (il dirigibile è pesante)
            if (_rb != null)
            {
                AddRecoil(rangedAttackConfig, activeSpawnPoints.Length);
            }
        }
        
        private Transform[] GetActiveSpawnPoints()
        {
            if (shootFromAllPoints)
            {
                return projectileSpawnPoints;
            }
            
            if (shootInSequence)
            {
                // Spara dal prossimo punto in sequenza
                Transform[] result = new Transform[1];
                result[0] = projectileSpawnPoints[_currentSpawnIndex];
                _currentSpawnIndex = (_currentSpawnIndex + 1) % projectileSpawnPoints.Length;
                return result;
            }
            
            // Spara da punti casuali
            int pointsToUse = maxPointsPerShot > 0 ? 
                Mathf.Min(maxPointsPerShot, projectileSpawnPoints.Length) : 
                projectileSpawnPoints.Length;
            
            Transform[] randomPoints = new Transform[pointsToUse];
            
            // Crea lista di indici disponibili
            var availableIndices = new System.Collections.Generic.List<int>();
            for (int i = 0; i < projectileSpawnPoints.Length; i++)
            {
                availableIndices.Add(i);
            }
            
            // Seleziona casualmente
            for (int i = 0; i < pointsToUse; i++)
            {
                int randomIndex = Random.Range(0, availableIndices.Count);
                randomPoints[i] = projectileSpawnPoints[availableIndices[randomIndex]];
                availableIndices.RemoveAt(randomIndex);
            }
            
            return randomPoints;
        }
        
        private void ShootFromPoint(Transform spawnPoint, RangedAttackConfig rangedAttackConfig)
        {
            // Calcola la direzione dal punto di spawn al target
            Vector2 shootDirection = _aimDirection.normalized;
            
            // Gestisce proiettili multipli (spread)
            float projectilesAngleSpace = rangedAttackConfig.multipleProjectilesAngle;
            float minAngle = -(rangedAttackConfig.numberOfProjectilesPerShot / 2f) * projectilesAngleSpace +
                           0.5f * rangedAttackConfig.multipleProjectilesAngle;

            for (int i = 0; i < rangedAttackConfig.numberOfProjectilesPerShot; i++)
            {
                float angle = minAngle + projectilesAngleSpace * i;
                float randomSpread = Random.Range(-rangedAttackConfig.spread, rangedAttackConfig.spread);
                angle += randomSpread;
                
                Vector2 finalDirection = RotateVector2(shootDirection, angle);
                
                // Spara il proiettile
                projectileManager.ShootBullet(spawnPoint.position, finalDirection, rangedAttackConfig);
            }
        }
        
        private static Vector2 RotateVector2(Vector2 v, float degrees)
        {
            return Quaternion.Euler(0, 0, degrees) * v;
        }
        
        private void AddRecoil(RangedAttackConfig rangedAttackConfig, int numberOfPoints)
        {
            // Recoil ridotto per dirigibile pesante e diviso per numero di punti
            float recoilForce = recoilStrength * 50f / numberOfPoints; // Ridotto da 100f a 50f
            _rb.AddForce(-(_aimDirection * recoilForce), ForceMode2D.Impulse);
        }
    }
}