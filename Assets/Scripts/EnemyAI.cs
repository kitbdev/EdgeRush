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
    // todo multiple phases based on health
    public class BossPhase {
        public Path path;
        public Vector3 pathOffset;
        public PatternSO attackPattern;
    }

    Health health;
    Rigidbody2D rb;

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
        EnemyManager.Instance.RemoveEnemy(this);
    }

    private void Update() {
        patternRunner?.ProcessPattern();
    }
}