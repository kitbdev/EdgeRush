using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {

    [SerializeField, ReadOnly] float _currentHealth;
    public float currentHealth {
        get { return _currentHealth; }
        protected set { _currentHealth = value; UpdateHealth(); }
    }
    public float maxHealth = 3;
    /// <summary>
    /// Seconds after a hit that future hits will be ignored
    /// </summary>
    public float hitInvincibleDur = 0.5f;
    public bool manualInvincible = false;
    public bool destroyOnDie = false;

    protected float lastDamageTime = 0;

    bool isHitInvincible => hitInvincibleDur > 0 && Time.time < lastDamageTime + hitInvincibleDur;
    public bool isInvincible => manualInvincible || isHitInvincible;
    // max health negative means true invincibility
    public bool isDead => currentHealth <= 0 && maxHealth >= 0;
    public bool isHealthFull => currentHealth >= maxHealth;
    public float healthPercent => currentHealth / maxHealth;

    [Header("Events")]
    public UnityEvent dieEvent;
    public UnityEvent damageEvent;
    public UnityEvent healthUpdateEvent;

    private void Awake() {
        RestoreHealth();
    }
    public void RestoreHealth() {
        currentHealth = maxHealth;
    }

    public void UpdateHealth() {
        healthUpdateEvent.Invoke();
    }

    public void Heal(float amount) {
        currentHealth += amount;
    }
    public void TakeDamage(float amount) {
        // Debug.Log($"{name} is hit for {amount}", this);
        if (isDead) {
            Debug.Log($"{name} is already dead", this);
            return;
        } else if (isInvincible) {
            // Debug.Log(name + " is invincible", this);
            return;
        }
        currentHealth -= amount;
        lastDamageTime = Time.time;
        damageEvent.Invoke();
        if (currentHealth <= 0) {
            Die();
        }
    }
    public void Die() {
        dieEvent.Invoke();
        if (destroyOnDie) {
            Destroy(gameObject);
        }
    }
}