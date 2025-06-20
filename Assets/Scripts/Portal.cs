using UnityEngine;
using System.Collections;
//This script only enables 1 script on the enemy, which is specified by the activeScriptName variable.
// The rest of the scripts on the enemy will be disabled until the enemy is spawned and moved to the portal.
// Also enables the animator.
public class Portal : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab; // The enemy prefab to spawn
    public Transform spawnPoint; // Where the enemy spawns (drag an empty GameObject)
    public string activeScriptName; // Name of the script that should remain active (e.g. "EnemyMovement")

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float activationDelay = 0.5f;
    public Vector3 moveDirection = Vector3.right;
    public float moveDistance = 3f; // <-- add this line

    [Header("Portal Animation")]
    public Animator portalAnimator; // Portal animator
    public string openTrigger = "Open"; // Trigger to open the portal
    public string closeTrigger = "Close"; // Trigger to close the portal
    public float portalOpenTime = 1f; // Time to open the portal before the enemy comes out
    public float portalCloseDelay = 2f; // Time before closing after the enemy comes out

    private bool isSpawning = false;

    void Start()
    {
        // Automatically start spawning when the portal is created
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

        // 1. Portal opening animation
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger(openTrigger);
        }

        // Wait for the portal to open
        yield return new WaitForSeconds(portalOpenTime);

        // 2. Spawn the enemy at the specified position
        Vector3 enemySpawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject spawnedEnemy = Instantiate(enemyPrefab, enemySpawnPos, Quaternion.identity);

        // Temporarily disable enemy scripts
        DisableEnemyScripts(spawnedEnemy);

        // 3. Move the enemy towards the portal
        yield return StartCoroutine(MoveEnemyToPortal(spawnedEnemy));

        // 4. Activate the enemy
        yield return new WaitForSeconds(activationDelay);
        EnableEnemyScripts(spawnedEnemy);

        // 5. Wait a bit and then close the portal
        yield return new WaitForSeconds(portalCloseDelay);

        // Portal closing animation
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger(closeTrigger);
        }

        // Wait for the closing animation to finish and then destroy the portal
        yield return new WaitForSeconds(1f); // Closing animation time

        Destroy(gameObject);
    }

    private IEnumerator MoveEnemyToPortal(GameObject enemy)
    {
        Vector3 startPos = enemy.transform.position;
        Vector3 targetPos;

        // If the direction is zero, fallback to the portal position
        if (moveDirection == Vector3.zero)
            targetPos = transform.position;
        else
            targetPos = startPos + moveDirection.normalized * moveDistance; // use moveDistance

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
        // Disable all MonoBehaviours on the enemy except the specified one and the Animator
        MonoBehaviour[] scripts = enemy.GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            // Do not disable SpriteRenderer, Animator, and the specified script
            if (!(script is SpriteRenderer) &&
                !(script is Animator) &&
                script.GetType().Name != activeScriptName)
            {
                script.enabled = false;
            }
        }

        // Disable Rigidbody2D
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        // Disable colliders
        Collider2D[] colliders = enemy.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }

    private void EnableEnemyScripts(GameObject enemy)
    {
        if (enemy == null) return;

        // Re-enable all MonoBehaviours
        MonoBehaviour[] scripts = enemy.GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = true;
        }

        // Re-enable Rigidbody2D
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
        }

        // Re-enable colliders
        Collider2D[] colliders = enemy.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = true;
        }
    }

    // Show the spawn position in the editor
    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);

            // Draw a line from the spawn point to the portal
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPoint.position, transform.position);
        }
    }
}