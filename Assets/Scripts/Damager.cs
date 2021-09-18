using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damager : MonoBehaviour {

    public float damageAmount = 0;
    public float knockbackStrength = 0;
    Bullet bullet;
    // public UnityEvent onHitEvent;

    private void Awake() {
        bullet = GetComponent<Bullet>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Debug.Log($"{name} hit {other.gameObject.name}");
        var health = other.gameObject.GetComponentInParent<Health>();
        if (health) {
            health.TakeDamage(damageAmount);
            // onHitEvent?.Invoke();
        }
        if (bullet) {
            BulletManager.Instance.RemoveBullet(bullet);
        }
        if (knockbackStrength > 0 && other.attachedRigidbody && other.attachedRigidbody.TryGetComponent<Player>(out var player)) {
            Vector2 dir = (other.attachedRigidbody.position - (Vector2)transform.position).normalized;
            // Debug.Log("knockback!" + dir*knockbackStrength);
            player.AddKnockback(dir * knockbackStrength);
        }
    }
    // private void OnTriggerStay2D(Collider2D other) {
        
    // }
}