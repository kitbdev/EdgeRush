using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager> {

    public Level[] levels = new Level[0];

    [SerializeField, ReadOnly] int _currentLevelIndex;
    public int currentLevelIndex => _currentLevelIndex;
    [SerializeField, ReadOnly] int curLevelEventIndex = 0;
    float lastEventTime = 0;

    private void OnValidate() {
        for (int i1 = 0; i1 < levels.Length; i1++) {
            Level level = levels[i1];
            level.Validate((i1 + 1) + "");
            for (int i = 0; i < level.levelEvents.Length; i++) {
                LevelEvent levelEvent = level.levelEvents[i];
                levelEvent.Validate((i + 1) + ". ");
            }
        }
    }

    private void Update() {
        ProcessLevel();
    }
    public void RestartLevel() {
        // Debug.Log("Restarting level " + currentLevelIndex);
        StartLevel(currentLevelIndex);
    }
    public void StartLevel(int levelIndex) {
        Debug.Log("Starting level " + levelIndex);
        ClearLevel();
        _currentLevelIndex = levelIndex;
        curLevelEventIndex = 0;
        if (currentLevelIndex >= levels.Length){
            // something
        }
    }
    void ClearLevel() {
        EnemyManager.Instance.RemoveAllEnemies();
        // todo debris and powerups clear
        // todo reset background?
        BulletManager.Instance.ClearAllActiveBullets();
        // todo player position reset?
    }
    void NextLevel() {
        Debug.Log($"Level {currentLevelIndex} finished!");
        StartLevel(_currentLevelIndex + 1);
        if (currentLevelIndex >= levels.Length) {
            Debug.Log("Finished all levels!");
        }
    }
    void ProcessLevel() {
        if (currentLevelIndex >= levels.Length) {
            return;
        }
        var curLevel = levels[currentLevelIndex];
        LevelEvent curEvent = curLevel.levelEvents[curLevelEventIndex];
        bool finished = HandleLevelEvent(curEvent);
        if (finished) {
            // move to the next event
            lastEventTime = Time.time;
            curLevelEventIndex++;
            if (curLevelEventIndex >= curLevel.levelEvents.Length) {
                // level is finished!
                NextLevel();
            }
        }
    }
    bool HandleLevelEvent(LevelEvent levelEvent) {
        switch (levelEvent.levelEventType) {
            case LevelEvent.LevelEventType.spawnEnemyWave:
                EnemyManager.Instance.SpawnWave(new EnemyManager.WaveSpawnData() {
                    prefab = levelEvent.spawnPrefab,
                    amount = levelEvent.amountToSpawn,
                    offset = levelEvent.spawnOffset,
                    offsetByIndex = levelEvent.spawnOffsetByIndex,
                    followPath = levelEvent.pathToFollow,
                    attackPatternOverride = levelEvent.attackPatternOverride,
                });
                break;
            case LevelEvent.LevelEventType.spawnBoss:
                // ?
                EnemyManager.Instance.SpawnWave(new EnemyManager.WaveSpawnData() {
                    prefab = levelEvent.spawnPrefab,
                    amount = 1,
                    offset = levelEvent.spawnOffset,
                    offsetByIndex = Vector2.zero,
                    followPath = levelEvent.pathToFollow,
                    attackPatternOverride = levelEvent.attackPatternOverride,
                });
                break;
            case LevelEvent.LevelEventType.spawnMisc:
                // todo
                break;
            case LevelEvent.LevelEventType.clearMap:
                if (levelEvent.clearEnemies) {
                    EnemyManager.Instance.RemoveAllEnemies();
                    // todo including bosses?
                }
                if (levelEvent.clearDebris) {
                    // todo
                }
                if (levelEvent.clearPowerups) {
                    // todo
                }
                if (levelEvent.clearBullets) {
                    BulletManager.Instance.ClearAllActiveBullets();
                }
                break;
            case LevelEvent.LevelEventType.waitDuration:
                return Time.time >= lastEventTime + levelEvent.waitDur;
            case LevelEvent.LevelEventType.waitEnemiesDefeated:
                return EnemyManager.Instance.numActiveEnemies == 0;
            case LevelEvent.LevelEventType.checkpoint:
                // todo
                break;
            case LevelEvent.LevelEventType.endLevel:
                NextLevel();
                break;
            default:
                // do nothing
                break;
        }
        // the event is finished processing
        return true;
    }
}