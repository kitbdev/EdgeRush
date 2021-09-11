using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {

    // shot pattern
    [SerializeField] WeaponSO[] weapons;

    [SerializeField, ReadOnly] WeaponSO curWeapon;

    // todo move pattern

    Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {

    }
}