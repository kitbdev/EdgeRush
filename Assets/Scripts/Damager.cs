using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damager : MonoBehaviour {

    public float damageAmount = 0;
    Bullet bullet;
    public UnityEvent onHitEvent;

    private void Awake() {
        bullet = GetComponent<Bullet>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Debug.Log($"{name} hit {other.gameObject.name}");
        var health = other.gameObject.GetComponentInParent<Health>();
        if (health) {
            health.TakeDamage(damageAmount);
            onHitEvent?.Invoke();
            if (bullet) {
                BulletManager.Instance.RemoveBullet(bullet);
            }
        }
    }
}