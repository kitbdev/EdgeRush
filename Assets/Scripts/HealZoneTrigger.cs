using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealZoneTrigger : MonoBehaviour {
    HealZones zones;
    private void Awake() {
        zones = GetComponentInParent<HealZones>();
    }
    private void OnTriggerStay2D(Collider2D other) {
        // Debug.Log(other.gameObject.name + " is in!");
        if (other.attachedRigidbody.CompareTag("Player")) {
            zones.Heal();
        }
    }
}