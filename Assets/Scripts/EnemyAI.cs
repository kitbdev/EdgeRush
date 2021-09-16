﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {

    [System.Serializable]
    public class DropRate {
        public WeaponSO weaponType;
        [Range(0f, 1f)]
        public float chance;
        public int ammo;
    }

    [System.Serializable]
    public class aiPhase {
        [Range(0f, 1f)]
        public float healthPercentTrigger;
        public Path newPath;
        public Vector3 newPathOffset;
        public PatternSO attackPattern;
    }

    [Header("Movement")]
    public Path path;
    public Vector3 pathOffset;

    [Header("Drop rates")]
    [Min(0)]
    public int numCoinsToDrop = 1;
    public DropRate[] dropRates = new DropRate[0];
    public AudioManager.AudioSettings deathAudio;

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
        deathAudio.position = transform.position;
        if (deathAudio != null) {
            AudioManager.Instance.PlaySfx(deathAudio);
        }
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
                if (droprate.weaponType != null) {
                    // is null for nothing
                    // drop this item
                    LevelManager.Instance.DropWeapon(droprate.weaponType, transform.position, droprate.ammo);
                }
                break;
            }
            acc += droprate.chance;
        }
    }

    private void Update() {
        patternRunner?.ProcessPattern();
    }
}