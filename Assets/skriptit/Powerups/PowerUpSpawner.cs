using System.Collections;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] powerUpPrefabs;

    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 12f;
    public int maxActive = 2;

    int activeCount = 0;

    void Start()
    {
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            if (activeCount < maxActive)
            {
                yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
                SpawnOne();
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void SpawnOne()
    {
        if (spawnPoints.Length == 0 || powerUpPrefabs.Length == 0) return;

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

        var go = Instantiate(prefab, sp.position, Quaternion.identity);
        var spawned = go.GetComponent<SpawnedPowerUp>();
        if (spawned == null) spawned = go.AddComponent<SpawnedPowerUp>();
        spawned.spawner = this;
        activeCount++;

    }

    public void OnPickupGone()
    {
        activeCount = Mathf.Max(0, activeCount - 1);
    }
}
