using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour {

    public WeaponSO weapon;

    private void OnTriggerEnter2D(Collider2D other) {
        var player = other.GetComponentInParent<Player>();
        if (player) {
            player.SetCurrentWeapon(weapon);
        }
    }
}