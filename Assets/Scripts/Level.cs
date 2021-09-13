using UnityEngine;

[System.Serializable]
public class Level {
    [SerializeField, HideInInspector] string title = "level";
    public void Validate(string prefix = "") {
        title = "Level " + prefix;
    }
    public int backgroundIndex;
    public AudioClip musicTrack;
    public LevelEvent[] levelEvents = new LevelEvent[0];
}
[System.Serializable]
public class LevelEvent {
    public enum LevelEventType {
        none,
        spawnEnemyWave,
        spawnBoss,
        spawnMisc,
        clearMap,
        waitDuration,
        waitEnemiesDefeated,
        checkpoint,
        endLevel,
    }

    [SerializeField, HideInInspector] string title = "level event";
    public void Validate(string prefix = "") {
        title = prefix + levelEventType.ToString();
    }

    public LevelEventType levelEventType;

    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.spawnEnemyWave,
                                            (int)LevelEventType.spawnBoss,
                                            (int)LevelEventType.spawnMisc)]
    public GameObject spawnPrefab;
    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.spawnEnemyWave,
                                            (int)LevelEventType.spawnMisc)]
    [Min(1)]
    public int amountToSpawn = 1;
    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.spawnEnemyWave,
                                            (int)LevelEventType.spawnMisc)]
    public Vector2 spawnOffset = Vector2.right;
    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.spawnEnemyWave,
                                            (int)LevelEventType.spawnMisc)]
    public Vector2 spawnOffsetByIndex = Vector2.right;
    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.spawnEnemyWave)]
    public Path pathToFollow;

    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.clearMap)]
    public bool clearEnemies = true;
    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.clearMap)]
    public bool clearDebris = true;
    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.clearMap)]
    public bool clearPowerups = true;
    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.clearMap)]
    public bool clearBullets = true;

    [Tooltip("time to wait in seconds")]
    [ConditionalHide(nameof(levelEventType), (int)LevelEventType.waitDuration)]
    public float waitDur = 0;

}