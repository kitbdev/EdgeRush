using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager> {

    public LevelSO[] levels = new LevelSO[0];

    [SerializeField, ReadOnly] int _currentLevelIndex;
    public int currentLevelIndex => _currentLevelIndex;
    int curLevelEventIndex = 0;
    float lastEventTime = 0;

    private void Update() {
        ProcessLevel();
    }
    void ProcessLevel() {
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
    void NextLevel() {
        Debug.Log($"Level {currentLevelIndex} finished!");
        _currentLevelIndex++;
        curLevelEventIndex = 0;
    }
    bool HandleLevelEvent(LevelEvent levelEvent) {
        switch (levelEvent.levelEventType) {
            case LevelEvent.LevelEventType.spawnEnemyWave:
                EnemyManager.Instance.SpawnWave(levelEvent.spawnPrefab, levelEvent.amountToSpawn, levelEvent.enemyOffsetByIndex);
                break;
            case LevelEvent.LevelEventType.spawnBoss:
                // todo
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