using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager> {

    public Level[] levels = new Level[0];
    public Level curLevel => currentLevelIndex >= 0 && currentLevelIndex < levels.Length ? levels[currentLevelIndex] : null;
    public string curLevelEventTitle => (curLevel != null && levelEventIndex < curLevel.levelEvents.Length) ?
        curLevel.levelEvents[levelEventIndex].Title : null;

    [SerializeField] GameObject coinprefab;
    [SerializeField] GameObject weaponpickupPrefab;
    [SerializeField] MeshRenderer bg;
    [SerializeField] ScrollingBackground scrollingBackground;
    [SerializeField] Player player;

    [Space]
    [SerializeField, ReadOnly] int _currentLevelIndex;
    public int currentLevelIndex => _currentLevelIndex;
    [SerializeField, ReadOnly] int levelEventIndex = 0;
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
    protected override void Awake() {
        base.Awake();
        _currentLevelIndex = -1;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    private void Start() {
        StartLevel(0);
    }

    private void Update() {
        ProcessLevel();
    }

    public void DropCoins(int amount, Vector3 position) {
        if (!coinprefab) return;
        for (int i = 0; i < amount; i++) {
            var go = Instantiate(coinprefab, transform);
            go.transform.position = position;
        }
    }
    public void DropWeapon(WeaponSO weaponType, Vector3 position) {
        if (!weaponpickupPrefab) return;
        var go = Instantiate(weaponpickupPrefab, transform);
        go.transform.position = position;
        go.GetComponent<ItemPickup>().weapon = weaponType;
    }

    public void StartGame() {
        StartLevel(0);
    }
    public void StopGame() {
        ClearLevel();
    }
    public void RestartLevel() {
        StartLevel(currentLevelIndex);
    }
    public void StartLevel(int levelIndex) {
        if (levelIndex < 0 || levelIndex >= levels.Length) {
            Debug.LogWarning("Invalid level " + (levelIndex + 1));
            return;
        }
        if (_currentLevelIndex == levelIndex) {
            // restarting level
            Debug.Log("Restarting level " + (levelIndex + 1));
        } else {
            Debug.Log("Starting level " + (levelIndex + 1));
        }
        ClearLevel();
        // todo level transition
        _currentLevelIndex = levelIndex;
        levelEventIndex = 0;
        Level level = levels[currentLevelIndex];
        if (level.backgroundMat && bg) {
            bg.sharedMaterial = level.backgroundMat;
        }
    }
    void ClearLevel() {
        scrollingBackground.ResetScrolls();
        EnemyManager.Instance.RemoveAllEnemies();
        // todo debris and powerups clear
        BulletManager.Instance.ClearAllActiveBullets();

        player.ResetForLevel();
    }
    void NextLevel() {
        Debug.Log($"Level {currentLevelIndex + 1} finished!");
        int nextLevelIndex = _currentLevelIndex + 1;
        // skip levels
        while (nextLevelIndex < levels.Length && levels[nextLevelIndex].skip) {
            nextLevelIndex++;
        }
        if (nextLevelIndex >= levels.Length) {
            Debug.Log("Finished all levels!");
            GameManager.Instance.PlayerWin();
            return;
        }
        StartLevel(nextLevelIndex);
    }
    void ProcessLevel() {
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Length) {
            return;
        }
        var curLevel = levels[currentLevelIndex];
        if (levelEventIndex < 0 || levelEventIndex >= curLevel.levelEvents.Length) {
            return;
        }
        int levInd = currentLevelIndex;
        LevelEvent curEvent = curLevel.levelEvents[levelEventIndex];
        bool finished = HandleLevelEvent(curEvent);
        if (finished) {
            // move to the next event
            lastEventTime = Time.time;
            if (levInd != currentLevelIndex) {
                // level was changed
            } else {
                levelEventIndex++;
            }
            if (levelEventIndex >= curLevel.levelEvents.Length) {
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
                var bossgo = EnemyManager.Instance.SpawnWave(new EnemyManager.WaveSpawnData() {
                    prefab = levelEvent.spawnPrefab,
                    amount = 1,
                    offset = levelEvent.spawnOffset,
                    offsetByIndex = Vector2.zero,
                    followPath = levelEvent.pathToFollow,
                    attackPatternOverride = levelEvent.attackPatternOverride,
                });
                HUDManager.Instance.SetBoss(bossgo);
                break;
            case LevelEvent.LevelEventType.spawnMisc:
                // todo test
                // give path?
                for (int i = 0; i < levelEvent.amountToSpawn; i++) {
                    var miscgo = Instantiate(levelEvent.spawnPrefab, transform);
                    Vector2 spawnPos = levelEvent.spawnOffset + levelEvent.spawnOffsetByIndex * i;
                    var rb = miscgo.GetComponent<Rigidbody2D>();
                    if (levelEvent.pathToFollow && rb) {
                        levelEvent.pathToFollow.FollowPath(rb, spawnPos, levelEvent.moveSpeedOverride);
                    } else {
                        miscgo.transform.position = spawnPos;
                    }
                }
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
                if (Time.time >= lastEventTime + levelEvent.waitDur) {
                    NextLevel();
                    return true;
                }
                return false;
            // break;
            default:
                // do nothing
                break;
        }
        // the event is finished processing
        return true;
    }
}