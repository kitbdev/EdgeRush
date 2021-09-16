using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class ItemPickup : MonoBehaviour {

    public enum PickupType {
        WEAPON, COIN
    }
    public PickupType pickupType;
    [ConditionalHide(nameof(pickupType), (int)PickupType.WEAPON)]
    public WeaponSO weapon;
    [ConditionalHide(nameof(pickupType), (int)PickupType.WEAPON)]
    public int ammoAmount = 1;
    [ConditionalHide(nameof(pickupType), (int)PickupType.COIN)]
    public int numCoins = 1;
    public Vector2 vel = Vector2.down;
    public AudioManager.AudioSettings pickupAudio;
    Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate() {
        rb.velocity = vel;
    }
    private void OnTriggerEnter2D(Collider2D other) {
        var player = other.GetComponentInParent<Player>();
        if (player) {
            if (pickupType == PickupType.WEAPON) {
                // player.SetCurrentWeapon(weapon);
                player.PickupWeaponAmmo(weapon, ammoAmount);
            } else if (pickupType == PickupType.COIN) {
                player.AddCoins(numCoins);
            }
            if (pickupAudio != null) {
                pickupAudio.position = transform.position;
                AudioManager.Instance.PlaySfx(pickupAudio);
            }
        }
        Destroy(gameObject);
    }
}