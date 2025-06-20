using UnityEngine;

namespace TopDownCharacter2D.Controllers
{
    /// <summary>
    /// Controllore per il dirigibile nemico - un miniboss che si avvicina lentamente e spara da più punti
    /// </summary>
    public class AirshipEnemyController : TopDownEnemyController
    {
        [Header("Airship Settings")]
        [SerializeField] private float followRange = 25f; // Più grande per un miniboss
        [SerializeField] private float optimalDistance = 12f; // Distanza a cui si ferma
        [SerializeField] private float stoppingTolerance = 1f; // Quanto può avvicinarsi alla distanza ottimale
        
        [Header("Attack Settings")]
        [SerializeField] private float burstFireRate = 0.2f; // Tempo tra colpi in una raffica
        [SerializeField] private int burstSize = 5; // Quanti colpi per raffica
        [SerializeField] private float burstCooldown = 2f; // Pausa tra raffiche
        
        private float lastBurstTime;
        private int shotsInCurrentBurst;
        private float lastShotTime;
        
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            float distance = DistanceToTarget();
            Vector2 direction = DirectionToTarget();

            // Controlla se il target è nel range
            if (distance <= followRange)
            {
                // Logica di movimento: avvicinati fino alla distanza ottimale
                if (distance > optimalDistance + stoppingTolerance)
                {
                    // Troppo lontano, avvicinati
                    OnMoveEvent.Invoke(direction);
                }
                else if (distance < optimalDistance - stoppingTolerance)
                {
                    // Troppo vicino, allontanati
                    OnMoveEvent.Invoke(-direction);
                }
                else
                {
                    // Alla distanza giusta, fermati
                    OnMoveEvent.Invoke(Vector2.zero);
                }
                
                // Logica di attacco con raffiche
                HandleBurstFire(direction);
            }
            else
            {
                // Target fuori range, fermati
                OnMoveEvent.Invoke(Vector2.zero);
                IsAttacking = false;
            }
        }
        
        private void HandleBurstFire(Vector2 direction)
        {
            float currentTime = Time.time;
            
            // Controlla se possiamo iniziare una nuova raffica
            if (shotsInCurrentBurst == 0 && currentTime - lastBurstTime >= burstCooldown)
            {
                // Inizia nuova raffica
                shotsInCurrentBurst = 0;
                lastBurstTime = currentTime;
            }
            
            // Controlla se possiamo sparare il prossimo colpo nella raffica
            if (shotsInCurrentBurst < burstSize && currentTime - lastShotTime >= burstFireRate)
            {
                // Raycast per controllare se c'è linea di vista
                int layerMaskTarget = Stats.CurrentStats.attackConfig.target;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, followRange,
                    (1 << LayerMask.NameToLayer("Level")) | layerMaskTarget);

                if (hit.collider != null &&
                    layerMaskTarget == (layerMaskTarget | (1 << hit.collider.gameObject.layer)))
                {
                    // Mira e spara
                    LookEvent.Invoke(direction);
                    IsAttacking = true;
                    
                    shotsInCurrentBurst++;
                    lastShotTime = currentTime;
                    
                    // Se abbiamo finito la raffica, resetta il contatore
                    if (shotsInCurrentBurst >= burstSize)
                    {
                        shotsInCurrentBurst = 0;
                    }
                }
                else
                {
                    IsAttacking = false;
                }
            }
            else
            {
                IsAttacking = false;
            }
        }
    }
}