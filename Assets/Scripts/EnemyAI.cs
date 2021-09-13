using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {

    public Path path;
    public Vector3 pathOffset;
    [SerializeField] PatternRunner attackPattern;

    // todo move pattern
    Health health;
    Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
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
    void OnDie() {
        EnemyManager.Instance.RemoveEnemy(this);
    }

    private void Update() {
        attackPattern.ProcessPattern();
    }
}