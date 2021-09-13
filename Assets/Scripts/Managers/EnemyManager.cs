﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager> {

    [SerializeField] List<GameObject> enemyPrefabs = new List<GameObject>();

    [ReadOnly] public List<EnemyAI> activeEnemies = new List<EnemyAI>();

    public int numActiveEnemies => activeEnemies.Count;

    MultiObjectPool enemyPool;

    protected override void Awake() {
        base.Awake();
        enemyPool = GetComponent<MultiObjectPool>();
        enemyPool.SetPrefabs(enemyPrefabs);
        enemyPool.forceAddPoolObjectComponent = true;
    }
    public Path epath;
    private void Start() {
        SpawnEnemy(0, epath);
    }
    void SpawnEnemy(int index, Path path) {
        int typeId = index + 1;
        var ego = enemyPool.Get(typeId);
        var enemyai = ego.GetComponent<EnemyAI>();
        activeEnemies.Add(enemyai);
        enemyai.path = path;
        ego.GetComponent<Health>().RestoreHealth();
        enemyai.OnSpawn();
    }
    public void RemoveAllEnemies() {
        for (int i = activeEnemies.Count - 1; i >= 0; i--) {
            RemoveEnemy(activeEnemies[i]);
        }
        activeEnemies.Clear();
    }
    /// <summary>
    /// call this to remove an enemy properly, when it dies
    /// </summary>
    /// <param name="enemy"></param>
    public void RemoveEnemy(EnemyAI enemy) {
        enemy.GetComponent<ObjectPoolObject>().RecycleFromPool();
        enemy.OnStop();
        activeEnemies.Remove(enemy);
    }
    public void SpawnWave(GameObject prefab, int amount, Vector2 offsetByIndex) {
        // todo
        int typeIndex = enemyPool.GetTypeId(prefab);
        for (int i = 0; i < amount; i++)
        {
            // SpawnEnemy(typeIndex)
        }
    }

}