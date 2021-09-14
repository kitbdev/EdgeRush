using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {

    // todo move pattern
    public Path path;
    public Vector3 pathOffset;
    [HideInInspector] public PatternRunner patternRunner;

    [System.Serializable]
    public class DropRate {
        public WeaponSO weaponType;
        [Range(0f, 1f)]
        public float chance;
    }
    [ContextMenuItem("Normalize Drop Rates", nameof(NormalizeDropRates))]
    public DropRate[] dropRates = new DropRate[0];

    // todo multiple phases based on health
    [System.Serializable]
    public class BossPhase {
        [Range(0f, 1f)]
        public float healthPercentTrigger;
        public Path path;
        public Vector3 pathOffset;
        public PatternSO attackPattern;
    }

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
    [ContextMenu("spawn")]
    public void OnSpawn() {
        // todo offset
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
        // todo
        NormalizeDropRates();
        // use droprates
        float val = Random.value;
        float acc = 0;
        foreach (var droprate in dropRates) {
            if (val >= acc && val < acc + droprate.chance) {
                // drop this item
                // todo
                // droprate.weaponType
                break;
            }
            acc += droprate.chance;
        }
    }

    private void Update() {
        patternRunner?.ProcessPattern();
    }
}