using System.Collections.Generic;
using UnityEngine;

namespace TopDownCharacter2D.Controllers
{
    /// <summary>
    ///     A floating contact enemy AI that follows the player while bobbing up and down.
    ///     This AI tries to go to the position of the nearest target under a certain distance 
    ///     to touch it and deal damage via a ChangeHealthOnTouch component, while maintaining
    ///     a floating animation and avoiding clustering with other enemies.
    /// </summary>
    public class TopDownFloatingEnemyController : TopDownEnemyController
    {
        [Header("Movement Settings")]
        [SerializeField] [Range(0f, 100f)] private float followRange = 15f;
        
        [Header("Flocking Settings")]
        [SerializeField] [Range(0f, 10f)] private float separationRange = 3f; // Distanza minima tra nemici
        [SerializeField] [Range(0f, 5f)] private float separationStrength = 2f; // Forza di separazione
        [SerializeField] [Range(0f, 2f)] private float alignmentStrength = 0.5f; // Forza di allineamento
        [SerializeField] [Range(0f, 1f)] private float cohesionStrength = 0.3f; // Forza di coesione
        [SerializeField] [Range(0f, 10f)] private float neighborRadius = 5f; // Raggio per trovare vicini
        [SerializeField] private LayerMask enemyLayerMask = 1; // Layer dei nemici
        
        [Header("Movement Smoothing")]
        [SerializeField] [Range(0f, 10f)] private float movementSmoothing = 5f; // Smoothing del movimento
        [SerializeField] [Range(0f, 2f)] private float randomMovementStrength = 0.2f; // Movimento casuale per varietà
        
        [Header("Floating Animation Settings")]
        [SerializeField] private float floatAmplitude = 0.5f; // Quanto in alto/basso fluttua
        [SerializeField] private float floatFrequency = 2f; // Velocità della fluttuazione
        [SerializeField] private Transform visualTransform; // Reference al transform dello sprite (MainSprite)
        
        private Vector3 _originalPosition;
        private float _floatTimer = 0f;
        private Vector2 _currentVelocity = Vector2.zero;
        private Vector2 _desiredDirection = Vector2.zero;
        private float _randomOffset; // Offset casuale per variare i comportamenti
        
        // Cache per ottimizzazione
        private List<Transform> _nearbyEnemies = new List<Transform>();
        private Collider2D[] _neighborBuffer = new Collider2D[20]; // Buffer per i vicini

        protected override void Awake()
        {
            base.Awake();
            
            // Genera un offset casuale per variare i comportamenti
            _randomOffset = Random.Range(0f, 100f);
            
            // Se non è stato assegnato manualmente, trova automaticamente il MainSprite
            if (visualTransform == null)
            {
                Transform mainSprite = transform.Find("MainSprite");
                if (mainSprite != null)
                {
                    visualTransform = mainSprite;
                }
                else
                {
                    // Se non trova MainSprite, usa il primo figlio o se stesso
                    visualTransform = transform.childCount > 0 ? transform.GetChild(0) : transform;
                }
            }
            
            // Salva la posizione originale relativa
            if (visualTransform != null)
            {
                _originalPosition = visualTransform.localPosition;
            }
            
            // Varia leggermente la frequenza di fluttuazione per ogni nemico
            floatFrequency += Random.Range(-0.3f, 0.3f);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            // Calcola la direzione desiderata combinando tutti i comportamenti
            Vector2 finalDirection = CalculateMovementDirection();
            
            // Applica smoothing al movimento per renderlo più fluido
            _currentVelocity = Vector2.Lerp(_currentVelocity, finalDirection, Time.fixedDeltaTime * movementSmoothing);
            
            // Invia il movimento al sistema
            OnMoveEvent.Invoke(_currentVelocity);
            
            // Gestisce l'animazione di fluttuazione
            UpdateFloatingAnimation();
        }

        /// <summary>
        /// Calcola la direzione di movimento combinando seguimento del target e flocking
        /// </summary>
        private Vector2 CalculateMovementDirection()
        {
            Vector2 finalDirection = Vector2.zero;
            
            // 1. Comportamento di base: seguire il target se è nel range
            Vector2 seekDirection = Vector2.zero;
            float distanceToTarget = DistanceToTarget();
            
            if (distanceToTarget < followRange && distanceToTarget > 0.1f)
            {
                seekDirection = DirectionToTarget();
            }
            
            // 2. Trova nemici vicini per i comportamenti di flocking
            UpdateNearbyEnemies();
            
            // 3. Calcola i comportamenti di flocking
            Vector2 separationForce = CalculateSeparation();
            Vector2 alignmentForce = CalculateAlignment();
            Vector2 cohesionForce = CalculateCohesion();
            
            // 4. Aggiunge un po' di movimento casuale per varietà
            Vector2 randomForce = CalculateRandomMovement();
            
            // 5. Combina tutte le forze con pesi appropriati
            finalDirection = seekDirection + 
                           separationForce * separationStrength + 
                           alignmentForce * alignmentStrength + 
                           cohesionForce * cohesionStrength +
                           randomForce * randomMovementStrength;
            
            // 6. Normalizza se necessario (mantieni intensità ragionevole)
            if (finalDirection.magnitude > 1f)
            {
                finalDirection = finalDirection.normalized;
            }
            
            return finalDirection;
        }

        /// <summary>
        /// Trova tutti i nemici vicini per i calcoli di flocking
        /// </summary>
        private void UpdateNearbyEnemies()
        {
            _nearbyEnemies.Clear();
            
            // Usa Physics2D.OverlapCircleNonAlloc per performance migliori
            int numFound = Physics2D.OverlapCircleNonAlloc(
                transform.position, 
                neighborRadius, 
                _neighborBuffer, 
                enemyLayerMask
            );
            
            for (int i = 0; i < numFound; i++)
            {
                Transform neighbor = _neighborBuffer[i].transform;
                if (neighbor != transform && neighbor.GetComponent<TopDownFloatingEnemyController>() != null)
                {
                    _nearbyEnemies.Add(neighbor);
                }
            }
        }

        /// <summary>
        /// Calcola la forza di separazione (evitare di ammassarsi)
        /// </summary>
        private Vector2 CalculateSeparation()
        {
            Vector2 separationForce = Vector2.zero;
            int count = 0;
            
            foreach (Transform neighbor in _nearbyEnemies)
            {
                float distance = Vector2.Distance(transform.position, neighbor.position);
                
                if (distance < separationRange && distance > 0.01f)
                {
                    // Calcola direzione di allontanamento
                    Vector2 diff = (Vector2)(transform.position - neighbor.position);
                    
                    // Più vicino = forza maggiore (inversamente proporzionale)
                    diff = diff.normalized / distance;
                    separationForce += diff;
                    count++;
                }
            }
            
            if (count > 0)
            {
                separationForce /= count;
                separationForce = separationForce.normalized;
            }
            
            return separationForce;
        }

        /// <summary>
        /// Calcola la forza di allineamento (muoversi nella stessa direzione dei vicini)
        /// </summary>
        private Vector2 CalculateAlignment()
        {
            Vector2 averageVelocity = Vector2.zero;
            int count = 0;
            
            foreach (Transform neighbor in _nearbyEnemies)
            {
                TopDownFloatingEnemyController neighborController = neighbor.GetComponent<TopDownFloatingEnemyController>();
                if (neighborController != null)
                {
                    averageVelocity += neighborController._currentVelocity;
                    count++;
                }
            }
            
            if (count > 0)
            {
                averageVelocity /= count;
                return averageVelocity.normalized;
            }
            
            return Vector2.zero;
        }

        /// <summary>
        /// Calcola la forza di coesione (rimanere vicino al gruppo)
        /// </summary>
        private Vector2 CalculateCohesion()
        {
            Vector2 centerOfMass = Vector2.zero;
            int count = 0;
            
            foreach (Transform neighbor in _nearbyEnemies)
            {
                centerOfMass += (Vector2)neighbor.position;
                count++;
            }
            
            if (count > 0)
            {
                centerOfMass /= count;
                Vector2 direction = (centerOfMass - (Vector2)transform.position).normalized;
                return direction;
            }
            
            return Vector2.zero;
        }

        /// <summary>
        /// Aggiunge movimento casuale per varietà
        /// </summary>
        private Vector2 CalculateRandomMovement()
        {
            float time = Time.time + _randomOffset;
            Vector2 randomDir = new Vector2(
                Mathf.PerlinNoise(time * 0.5f, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, time * 0.5f) - 0.5f
            );
            
            return randomDir.normalized;
        }

        /// <summary>
        /// Aggiorna l'animazione di fluttuazione del nemico
        /// </summary>
        private void UpdateFloatingAnimation()
        {
            if (visualTransform == null) return;
            
            // Incrementa il timer per l'animazione con offset casuale
            _floatTimer += Time.fixedDeltaTime * floatFrequency;
            
            // Calcola l'offset verticale usando una funzione sinusoidale con offset
            float verticalOffset = Mathf.Sin(_floatTimer + _randomOffset) * floatAmplitude;
            
            // Applica l'offset alla posizione locale del visual transform
            Vector3 newPosition = _originalPosition;
            newPosition.y += verticalOffset;
            visualTransform.localPosition = newPosition;
        }

        /// <summary>
        /// Resetta la posizione di fluttuazione (utile se serve)
        /// </summary>
        public void ResetFloatingPosition()
        {
            if (visualTransform != null)
            {
                visualTransform.localPosition = _originalPosition;
            }
            _floatTimer = 0f;
        }

        /// <summary>
        /// Imposta i parametri di fluttuazione a runtime
        /// </summary>
        /// <param name="amplitude">Ampiezza della fluttuazione</param>
        /// <param name="frequency">Frequenza della fluttuazione</param>
        public void SetFloatingParameters(float amplitude, float frequency)
        {
            floatAmplitude = amplitude;
            floatFrequency = frequency;
        }

        /// <summary>
        /// Imposta i parametri di flocking a runtime
        /// </summary>
        public void SetFlockingParameters(float separationRange, float separationStrength, float alignmentStrength, float cohesionStrength)
        {
            this.separationRange = separationRange;
            this.separationStrength = separationStrength;
            this.alignmentStrength = alignmentStrength;
            this.cohesionStrength = cohesionStrength;
        }

        // Debug: disegna i range nel Scene View
        private void OnDrawGizmosSelected()
        {
            // Range di seguito
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, followRange);

            // Range di separazione
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, separationRange);

            // Range di rilevamento vicini
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, neighborRadius);

            // Linee verso i vicini
            if (_nearbyEnemies != null)
            {
                Gizmos.color = Color.cyan;
                foreach (Transform neighbor in _nearbyEnemies)
                {
                    if (neighbor != null)
                    {
                        Gizmos.DrawLine(transform.position, neighbor.position);
                    }
                }
            }
        }
    }
}