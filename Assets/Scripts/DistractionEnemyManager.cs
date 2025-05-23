using System.Collections.Generic;
using UnityEngine;

public class DistractionEnemyManager : MonoBehaviour
{
    public GameObject distractionPrefab;
    public int poolSize = 10;
    public float spawnInterval = 5f;
    private float spawnTimer = 0f;
    private float startTime;

    private List<DistractionEnemy> enemyPool = new List<DistractionEnemy>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(distractionPrefab, transform);
            obj.transform.SetParent(transform);
            DistractionEnemy enemy = obj.GetComponent<DistractionEnemy>();
            enemyPool.Add(enemy);
        }
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime < 1f) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        foreach (var enemy in enemyPool)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                enemy.Activate();
                return;
            }
        }
    }
}
