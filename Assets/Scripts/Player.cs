using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Player : MonoBehaviour {

    [SerializeField] float boundMaxX = 10;
    [SerializeField] float boundMaxY = 0;
    [SerializeField] float boundMinY = -5;
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float accelerationRate = 2;
    [SerializeField] Transform shootPoint;
    [SerializeField] Transform[] shootPoints = new Transform[0];
    [SerializeField] WeaponSO currentWeapon;

    [SerializeField, ReadOnly] Vector2 velocity = Vector2.zero;
    [SerializeField, ReadOnly] float lastShootTime = 0;

    Controls controls;
    [SerializeField, ReadOnly] Vector2 inputMove = Vector2.zero;
    [SerializeField, ReadOnly] bool inputShoot;
    [SerializeField, ReadOnly] bool inputShootHold;

    Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnEnable() {
        controls = new Controls();
        controls.Enable();
        controls.Player.Move.performed += c => inputMove = c.ReadValue<Vector2>();
        controls.Player.Move.canceled += c => inputMove = Vector2.zero;
        controls.Player.Fire.performed += c => { inputShootHold = true; inputShoot = true; };
        controls.Player.Fire.canceled += c => { inputShootHold = false; };
    }
    private void OnDisable() {
        controls?.Disable();
    }
    private void Update() {
        if (Time.timeScale == 0) return;
        if (inputShootHold) {
            if (Time.time > lastShootTime + currentWeapon.shootHoldCooldownDur) {
                ShootCurWeapon();
            }
        } else if (inputShoot) {
            if (Time.time > lastShootTime + currentWeapon.shootCooldownDur) {
                ShootCurWeapon();
            }
        }
    }
    void ShootCurWeapon() {
        Transform[] curShootPoints = new Transform[currentWeapon.numShootPoints];
        // todo check
        switch (curShootPoints.Length) {
            case 1:
                curShootPoints[0] = shootPoints[0];
                break;
                // case 2:
                //     curShootPoints[0] = shootPoints[1];
                //     curShootPoints[1] = shootPoints[2];
                //     break;
                // case 3:
                //     curShootPoints[0] = shootPoints[0];
                //     curShootPoints[1] = shootPoints[1];
                //     curShootPoints[2] = shootPoints[2];
                // break;
        }
        currentWeapon.Shoot(curShootPoints, true);
        lastShootTime = Time.time;
        inputShoot = false;
    }
    private void FixedUpdate() {
        if (Time.timeScale == 0) return;
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