using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] float moveSpeed = 2;

    Rigidbody2D rb;

    Controls controls;
    Vector2 inputMove;
    bool inputShoot;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        controls = new Controls();
        controls.Enable();
        controls.Player.Move.performed += c => inputMove = c.ReadValue<Vector2>();
        controls.Player.Move.canceled += c => inputMove = Vector2.zero;
    }
    private void FixedUpdate() {
        // move
        // todo acceleration
        // todo bounds
        Vector2 vel = inputMove * moveSpeed;
        rb.velocity = vel;
    }

}