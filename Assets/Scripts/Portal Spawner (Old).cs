using UnityEngine;
using System.Collections;

public class PortalSpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab; // Il prefab del nemico da spawnare
    public Transform spawnPoint; // Dove spawna il nemico (trascina un GameObject vuoto)
    
    [Header("Movement Settings")]
    public float moveSpeed = 3f; // Velocità di movimento verso il portale
    public float activationDelay = 0.5f; // Ritardo prima di attivare gli script del nemico
    public Vector3 moveDirection = Vector3.right; // Direzione di movimento dopo lo spawn
    
    [Header("Portal Animation")]
    public Animator portalAnimator; // Animator del portale
    public string openTrigger = "Open"; // Trigger per aprire il portale
    public string closeTrigger = "Close"; // Trigger per chiudere il portale
    public float portalOpenTime = 1f; // Tempo per aprire il portale prima che esca il nemico
    public float portalCloseDelay = 2f; // Tempo prima che si chiuda dopo che il nemico è uscito
    
    private bool isSpawning = false;
    
    void Start()
    {
        // Avvia automaticamente lo spawn quando il portale viene creato
        SpawnEnemy();
    }
    
    public void SpawnEnemy()
    {
        if (isSpawning) return;
        
        StartCoroutine(SpawnEnemyCoroutine());
    }
    
    private IEnumerator SpawnEnemyCoroutine()
    {
        isSpawning = true;
        
        // 1. Animazione di apertura portale
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger(openTrigger);
        }
        
        // Aspetta che il portale si apra
        yield return new WaitForSeconds(portalOpenTime);
        
        // 2. Spawna il nemico nella posizione specificata
        Vector3 enemySpawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject spawnedEnemy = Instantiate(enemyPrefab, enemySpawnPos, Quaternion.identity);
        
        // Disabilita temporaneamente gli script del nemico
        DisableEnemyScripts(spawnedEnemy);
        
        // 3. Muovi il nemico verso il portale
        yield return StartCoroutine(MoveEnemyToPortal(spawnedEnemy));
        
        // 4. Attiva il nemico
        yield return new WaitForSeconds(activationDelay);
        EnableEnemyScripts(spawnedEnemy);
        
        // 5. Aspetta un po' e poi chiudi il portale
        yield return new WaitForSeconds(portalCloseDelay);
        
        // Animazione di chiusura portale
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger(closeTrigger);
        }
        
        // Aspetta che l'animazione di chiusura finisca e poi distruggi il portale
        yield return new WaitForSeconds(1f); // Tempo dell'animazione di chiusura
        
        Destroy(gameObject);
    }
    
    private IEnumerator MoveEnemyToPortal(GameObject enemy)
    {
        Vector3 startPos = enemy.transform.position;
        Vector3 targetPos;

        // Se la direzione è nulla, fallback alla posizione del portale
        if (moveDirection == Vector3.zero)
            targetPos = transform.position;
        else
            targetPos = startPos + moveDirection.normalized * 3f; // 3f = distanza arbitraria, puoi modificarla

        float journeyTime = Vector3.Distance(startPos, targetPos) / moveSpeed;
        float elapsedTime = 0;

        while (elapsedTime < journeyTime)
        {
            if (enemy == null) yield break;

            elapsedTime += Time.deltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;

            enemy.transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);

            yield return null;
        }

        if (enemy != null)
        {
            enemy.transform.position = targetPos;
        }
    }
    
    private void DisableEnemyScripts(GameObject enemy)
    {
        // Disabilita tutti i MonoBehaviour del nemico
        MonoBehaviour[] scripts = enemy.GetComponentsInChildren<MonoBehaviour>();
        
        foreach (MonoBehaviour script in scripts)
        {
            if (!(script is SpriteRenderer) && !(script is Animator))
            {
                script.enabled = false;
            }
        }
        
        // Disabilita Rigidbody2D
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
        
        // Disabilita i collider
        Collider2D[] colliders = enemy.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }
    
    private void EnableEnemyScripts(GameObject enemy)
    {
        if (enemy == null) return;
        
        // Riabilita tutti i MonoBehaviour
        MonoBehaviour[] scripts = enemy.GetComponentsInChildren<MonoBehaviour>();
        
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = true;
        }
        
        // Riabilita Rigidbody2D
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
        }
        
        // Riabilita i collider
        Collider2D[] colliders = enemy.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = true;
        }
    }
    
    // Visualizza la posizione di spawn nell'editor
    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            
            // Disegna una linea dal punto di spawn al portale
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPoint.position, transform.position);
        }
    }

    // Questo metodo verrà chiamato dall'Animation Event
    public void OnPortalCloseAnimationEnd()
    {
        Destroy(gameObject);
    }
}