using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {

    [Header("Movement")]
    public Path path;
    public Vector3 pathOffset;

    [System.Serializable]
    public class DropRate {
        public WeaponSO weaponType;
        [Range(0f, 1f)]
        public float chance;
    }
    [Header("Drop rates")]
    [Min(0)]
    public int numCoinsToDrop = 1;
    public DropRate[] dropRates = new DropRate[0];

    [System.Serializable]
    public class aiPhase {
        [Range(0f, 1f)]
        public float healthPercentTrigger;
        public Path newPath;
        public Vector3 newPathOffset;
        public PatternSO attackPattern;
    }
    // todo multiple phases based on health
    [SerializeField] aiPhase[] phases = new aiPhase[0];

    PatternRunner patternRunner;
    Health health;
    Rigidbody2D rb;

    [ContextMenu("NormalizeDropRates")]
    void NormalizeDropRates() {
        float total = 0;
        foreach (var droprate in dropRates) {
            total += droprate.chance;
        }
        foreach (var droprate in dropRates) {
            droprate.chance /= total;
        }
    }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        patternRunner = GetComponent<PatternRunner>();
        health = GetComponent<Health>();
        health.destroyOnDie = false;
        health.dieEvent.AddListener(OnDie);
    }
    public void SetAttackPattern(PatternSO attackPattern) {
        patternRunner.patternSO = attackPattern;
    }
    [ContextMenu("spawn")]
    public void OnSpawn() {
        path?.FollowPath(rb, pathOffset);
    }
    public void OnStop() {
        path?.StopPath();
    }
    [ContextMenu("Kill")]
    void OnDie() {
        DropItem();
        EnemyManager.Instance.RemoveEnemy(this);
    }
    void DropItem() {
        // drop coins
        LevelManager.Instance.DropCoins(numCoinsToDrop, transform.position);
        // chance to drop weapon
        NormalizeDropRates();
        float val = Random.value;
        float acc = 0;
        foreach (var droprate in dropRates) {
            if (val >= acc && val < acc + droprate.chance) {
                // drop this item
                LevelManager.Instance.DropWeapon(droprate.weaponType, transform.position);
                break;
            }
            acc += droprate.chance;
        }
    }

    private void Update() {
        patternRunner?.ProcessPattern();
    }
}