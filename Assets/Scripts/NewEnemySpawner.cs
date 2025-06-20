using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Il nome di questa classe non crea conflitti, quindi può rimanere invariato.
// Se preferisci, potresti chiamarla "ModularSpawnRule" per coerenza.
[System.Serializable]
public class SpawnRule
{
    [Tooltip("Un nome per riconoscere questa regola nell'Inspector (es. 'Goblin Veloci dal Bosco')")]
    public string ruleName = "Nuova Regola di Spawn";
    
    [Header("Configurazione Nemico")]
    public GameObject enemyPrefab;
    
    [Tooltip("Intervallo di base in secondi tra gli spawn.")]
    public float spawnInterval = 5f;
    
    [Tooltip("Aggiunge casualità all'intervallo. Es: 1 significa che l'intervallo varierà di +/- 0.5s.")]
    public float intervalRandomness = 1f;
    
    [Header("Configurazione Spawn Points")]
    [Tooltip("I punti di spawn dedicati a QUESTA specifica regola.")]
    public Transform[] spawnPoints;
}

// ============== CAMBIO NOME PRINCIPALE QUI ==============
// La classe ora si chiama "ModularEnemySpawner" per distinguerla dall'originale.
public class ModularEnemySpawner : MonoBehaviour
{
    // La lista usa la classe "SpawnRule" definita sopra.
    [SerializeField]
    private List<SpawnRule> spawnRules;

    void Start()
    {
        // La logica interna non ha bisogno di cambiamenti.
        foreach (var rule in spawnRules)
        {
            if (rule.enemyPrefab != null && rule.spawnPoints != null && rule.spawnPoints.Length > 0)
            {
                StartCoroutine(SpawnEnemyCoroutine(rule));
            }
            else
            {
                Debug.LogWarning($"Attenzione: La regola di spawn '{rule.ruleName}' nel ModularEnemySpawner non è configurata correttamente e verrà ignorata.");
            }
        }
    }

    private IEnumerator SpawnEnemyCoroutine(SpawnRule rule)
    {
        while (true)
        {
            float randomDelay = rule.spawnInterval + Random.Range(-rule.intervalRandomness / 2f, rule.intervalRandomness / 2f);
            float waitTime = Mathf.Max(0.1f, randomDelay);
            
            yield return new WaitForSeconds(waitTime);

            Transform randomSpawnPoint = rule.spawnPoints[Random.Range(0, rule.spawnPoints.Length)];

            if (randomSpawnPoint != null)
            {
                Instantiate(rule.enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
            }
        }
    }
}