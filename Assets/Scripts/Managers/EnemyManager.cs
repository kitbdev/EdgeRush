using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager> {

    [SerializeField] List<GameObject> enemyPrefabs = new List<GameObject>();

    [SerializeField, ReadOnly] List<EnemyAI> activeEnemies = new List<EnemyAI>();

    public int numActiveEnemies => activeEnemies.Count;

    MultiObjectPool enemyPool;

    protected override void Awake() {
        base.Awake();
        enemyPool = GetComponent<MultiObjectPool>();
        enemyPool.SetPrefabs(enemyPrefabs);
        enemyPool.forceAddPoolObjectComponent = true;
    }

    void SpawnEnemy(int typeId, Path path, Vector2 offset, PatternSO patternOverride = null) {
        var ego = enemyPool.Get(typeId);
        var enemyai = ego.GetComponent<EnemyAI>();
        activeEnemies.Add(enemyai);
        enemyai.path = path;
        enemyai.pathOffset = offset;
        if (patternOverride != null) {
            enemyai.SetAttackPattern(patternOverride);
        }
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
        enemy.OnStop();
        enemy.GetComponent<ObjectPoolObject>().RecycleFromPool();
        activeEnemies.Remove(enemy);
    }
    public struct WaveSpawnData {
        public GameObject prefab;
        public int amount;
        public Vector2 offset;
        public Vector2 offsetByIndex;
        public Path followPath;
        public PatternSO attackPatternOverride;
    }
    public GameObject SpawnWave(WaveSpawnData waveSpawnData) {
        int typeIndex = enemyPool.GetTypeId(waveSpawnData.prefab);
        for (int i = 0; i < waveSpawnData.amount; i++) {
            Vector2 offset = waveSpawnData.offset + waveSpawnData.offsetByIndex * i;
            SpawnEnemy(typeIndex, waveSpawnData.followPath, offset, waveSpawnData.attackPatternOverride);
        }
        return activeEnemies.Count > 0 ? activeEnemies[activeEnemies.Count - 1].gameObject : null;
    }

}