using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour {

    public float damageAmount = 0;
    public ObjectPoolObject recycleObject;

    private void Awake() {
        recycleObject ??= GetComponent<ObjectPoolObject>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Debug.Log($"{name} hit {other.gameObject.name}");
        var health = other.gameObject.GetComponentInParent<Health>();
        if (health) {
            health.TakeDamage(damageAmount);
            recycleObject.RecycleFromPool();
        }
    }
}