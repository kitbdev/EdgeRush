using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] float boundMaxX = 10;
    [SerializeField] float boundMaxY = 0;
    [SerializeField] float boundMinY = -5;
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float accelerationRate = 2;
    [SerializeField] Transform shootPoint;
    [SerializeField] Transform shootPrefab;// todo

    Vector2 velocity = Vector2.zero;

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
        Vector2 nvel = Vector2.ClampMagnitude(inputMove, 1f) * moveSpeed;
        velocity = Vector2.Lerp(velocity, nvel, accelerationRate * Time.deltaTime);
        // if (velocity.sqrMagnitude > 0.001f) {
        // }
        rb.velocity = velocity;

        // keep player in bounds
        Vector3 pos = transform.position;
        if (pos.x > boundMaxX) {
            pos.x = boundMaxX;
            transform.position = pos;
        } else if (pos.x < -boundMaxX) {
            pos.x = -boundMaxX;
            transform.position = pos;
        }
        pos = transform.position;
        if (pos.y > boundMaxY) {
            pos.y = boundMaxY;
            transform.position = pos;
        } else if (pos.y < boundMinY) {
            pos.y = boundMinY;
            transform.position = pos;
        }

    }
    private void OnDrawGizmosSelected() {
        Vector3 topleft = new Vector3(-boundMaxX, boundMaxY);
        Vector3 topright = new Vector3(boundMaxX, boundMaxY);
        Vector3 bottomleft = new Vector3(-boundMaxX, boundMinY);
        Vector3 bottomright = new Vector3(boundMaxX, boundMinY);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(topleft, topright);
        Gizmos.DrawLine(topleft, bottomleft);
        Gizmos.DrawLine(bottomright, bottomleft);
        Gizmos.DrawLine(bottomright, topright);
    }

}