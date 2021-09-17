using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager> {

    [System.Serializable]
    class LevelCheckpointData {
        // no enemies
        // no items or bullets on screen
        public int level;
        public int levelEvent;
        public float playerHealth;
        public Player.WeaponData[] weaponDatas;
        public int coinAmount;
        public int playerSelectedWeapon;
        public static LevelCheckpointData Save(int level, int levelEvent, Player player) {
            LevelCheckpointData cpdata = new LevelCheckpointData();
            cpdata.level = level;
            cpdata.levelEvent = levelEvent;
            cpdata.coinAmount = player.numCoins;
            cpdata.playerSelectedWeapon = player.curSelectedWeapon;
            cpdata.playerHealth = player.GetComponent<Health>().currentHealth;
            cpdata.weaponDatas = player.weaponDatas.ToArray();
            return cpdata;
        }
    }

    public LevelSO[] levels = new LevelSO[0];
    public LevelSO curLevel => currentLevelIndex >= 0 && currentLevelIndex < levels.Length ? levels[currentLevelIndex] : null;
    public string curLevelEventTitle => (curLevel != null && levelEventIndex < curLevel.levelEvents.Length) ?
        curLevel.levelEvents[levelEventIndex].Title : null;

    [SerializeField] GameObject coinprefab;
    [SerializeField] GameObject weaponpickupPrefab;
    [SerializeField] MeshRenderer bg;
    [SerializeField] ScrollingBackground scrollingBackground;
    [SerializeField] Player player;
    [SerializeField] AnimPlayer levelTransitionAnimPlayer;
    [SerializeField] float levelTransitionDuration = 5f;
    [SerializeField] float levelTransitionHappenTime = 2f;
    [SerializeField, ReadOnly] bool isChangingLevels = false;
    [SerializeField, ReadOnly] float changeLevelStartTime = 0;
    [SerializeField, ReadOnly] bool changedLevels = false;


    [SerializeField, ReadOnly] LevelCheckpointData lastCheckPoint;

    [Header("Debug")]
    [SerializeField] bool debugLoggingEditor = false;

#pragma warning disable 0219
    [SerializeField] int startLevelEditor = 0;
    [SerializeField] int startLevelEventEditor = 0;
#pragma warning restore 0219

    [Space]
    [SerializeField, ReadOnly] int _currentLevelIndex;
    public int currentLevelIndex => _currentLevelIndex;
    [SerializeField, ReadOnly] int levelEventIndex = 0;
    float lastEventTime = 0;

    private void OnValidate() {
        for (int i1 = 0; i1 < levels.Length; i1++) {
            LevelSO level = levels[i1];
            if (level == null) continue;
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
        // StartLevel(0);
    }

    private void Update() {
        if (isChangingLevels) {
            if (changedLevels && Time.time > changeLevelStartTime + levelTransitionDuration) {
                // we are done
                isChangingLevels = false;
                levelTransitionAnimPlayer.Stop();
            } else if (!changedLevels && Time.time > changeLevelStartTime + levelTransitionHappenTime) {
                // actually change the level
                // Debug.Break();
                changedLevels = true;
                NextLevelSet();
                return;
            } else {
                return;
            }
        }
        ProcessLevel();
    }

    public void SaveCheckPoint() {
        lastCheckPoint = LevelCheckpointData.Save(currentLevelIndex, levelEventIndex, player);
    }
    public bool LoadCheckPoint() {
        if (lastCheckPoint == null) {
            return false;
        }
        ClearLevel();
        player.ResetAll();
        player.AddCoins(lastCheckPoint.coinAmount);
        player.GetComponent<Health>().SetHealth(lastCheckPoint.playerHealth);
        player.weaponDatas = new List<Player.WeaponData>(lastCheckPoint.weaponDatas);
        player.curSelectedWeapon = lastCheckPoint.playerSelectedWeapon;
        _currentLevelIndex = lastCheckPoint.level;
        levelEventIndex = lastCheckPoint.levelEvent;
        HUDManager.Instance.UpdateAll();
        return true;
    }
    public void DropCoins(int amount, Vector3 position) {
        if (!coinprefab) return;
        for (int i = 0; i < amount; i++) {
            var go = Instantiate(coinprefab, transform);
            Vector3 rpos = (amount > 0) ? (Vector3)Random.insideUnitCircle : Vector3.zero;
            go.transform.position = position + rpos;
        }
    }
    public void DropWeapon(WeaponSO weaponType, Vector3 position, int ammoamount) {
        if (!weaponpickupPrefab) return;
        var go = Instantiate(weaponpickupPrefab, transform);
        go.transform.position = position;
        ItemPickup itemPickup = go.GetComponent<ItemPickup>();
        itemPickup.weapon = weaponType;
        itemPickup.ammoAmount = ammoamount;
    }

    public void StartGame() {
        // DG.Tweening.DOTween.KillAll();
        player.ResetAll();
#if UNITY_EDITOR
        StartLevel(startLevelEditor);
        levelEventIndex = startLevelEventEditor;
#else
        StartLevel(0);
#endif
        PauseManager.Instance.blockPause = false;
    }
    public void StopGame() {
        ClearLevel();
    }
    public void RetryLevel() {
        bool checkpoint = LoadCheckPoint();
        if (!checkpoint) {
            // ? go back to original bullet and coin counts
            player.GetComponent<Health>().RestoreHealth();
            RestartLevel();
        }
    }
    public void RestartLevel() {
        DG.Tweening.DOTween.KillAll();
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
        _currentLevelIndex = levelIndex;
        levelEventIndex = 0;
        LevelSO level = levels[currentLevelIndex];
        if (level.backgroundMat && bg) {
            bg.sharedMaterial = level.backgroundMat;
        }
        SaveCheckPoint();
    }
    void ClearLevel() {
        scrollingBackground.ResetScrolls();
        EnemyManager.Instance.RemoveAllEnemies();
        ClearLevelDebris();
        BulletManager.Instance.ClearAllActiveBullets();

        player.ResetForLevel();
    }
    void ClearLevelDebris() {
        // clears debris, powerups, and coins
        ClearChildren();
    }
    void ClearChildren() {
        int numChildren = transform.childCount;
        for (int i = numChildren - 1; i >= 0; i--) {
            var go = transform.GetChild(i).gameObject;
            if (go.TryGetComponent<PathRunHandler>(out var pathRunHandler)) {
                pathRunHandler.KillSequence();
            }
            if (Application.isPlaying) {
                Destroy(go);
            } else {
                DestroyImmediate(go);
            }
        }
    }
    void NextLevel() {
        Debug.Log($"Level {currentLevelIndex + 1} finished!");
        changedLevels = false;
        isChangingLevels = true;
        changeLevelStartTime = Time.time;
        levelTransitionAnimPlayer.Play();
    }
    void NextLevelSet() {
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
        if (debugLoggingEditor) {
            Debug.Log($"starting event {levelEventIndex} {curLevelEventTitle}");
        }
        bool finished = HandleLevelEvent(curEvent);
        if (finished && !isChangingLevels) {
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
                // give path?
                for (int i = 0; i < levelEvent.amountToSpawn; i++) {
                    var miscgo = Instantiate(levelEvent.spawnPrefab, transform);
                    Vector2 spawnPos = levelEvent.spawnOffset + levelEvent.spawnOffsetByIndex * i;
                    var rb = miscgo.GetComponent<Rigidbody2D>();
                    if (levelEvent.pathToFollow && rb) {
                        var seq = levelEvent.pathToFollow.FollowPath(rb, spawnPos, levelEvent.moveSpeedOverride, null);
                        if (miscgo.TryGetComponent<PathRunHandler>(out var pathRunHandler)) {
                            pathRunHandler.sequence = seq;
                        }
                    } else {
                        miscgo.transform.position = spawnPos;
                    }
                }
                break;
            case LevelEvent.LevelEventType.clearMap:
                if (levelEvent.clearEnemies) {
                    // note this includes bosses
                    EnemyManager.Instance.RemoveAllEnemies();
                }
                if (levelEvent.clearDebris) {
                    ClearLevelDebris();
                }
                // if (levelEvent.clearPowerups) {
                //     // todo
                // }
                if (levelEvent.clearBullets) {
                    BulletManager.Instance.ClearAllActiveBullets();
                }
                break;
            case LevelEvent.LevelEventType.waitDuration:
                return Time.time >= lastEventTime + levelEvent.waitDur;
            case LevelEvent.LevelEventType.waitEnemiesDefeated:
                return EnemyManager.Instance.numActiveEnemies == 0;
            case LevelEvent.LevelEventType.checkpoint:
                SaveCheckPoint();
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