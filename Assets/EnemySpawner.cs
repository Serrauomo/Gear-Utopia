using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject swarmerPrefab;
    [SerializeField]
    private float swarmerInterval = 3.5f;

    [SerializeField]
    private GameObject mediumSwarmerPrefab;
    [SerializeField]
    private float mediumSwarmerInterval = 7f;
    
    [SerializeField]
    private GameObject bigSwarmerPrefab;
    [SerializeField]
    private float bigSwarmerInterval = 10f;

    void Start()
    {
        StartCoroutine(spawnEnemy(swarmerInterval, swarmerPrefab));
        StartCoroutine(spawnEnemy(mediumSwarmerInterval, mediumSwarmerPrefab));
        StartCoroutine(spawnEnemy(bigSwarmerInterval, bigSwarmerPrefab));
    }

    private IEnumerator spawnEnemy(float interval, GameObject enemy)
    {
        yield return new WaitForSeconds(interval);
        GameObject newEnemy = Instantiate(enemy, new Vector3(Random.Range(-30f, 30), Random.Range(-30f, 30f), 0), Quaternion.identity);
        StartCoroutine(spawnEnemy(interval, enemy));
    }
}
