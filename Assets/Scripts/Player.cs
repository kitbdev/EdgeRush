using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Player : MonoBehaviour {

    [SerializeField] float moveSpeed = 10;
    [SerializeField] float accelerationRate = 2;
    [SerializeField] Transform moveTo;

    [SerializeField] Transform[] shootPoints = new Transform[0];
    [SerializeField] WeaponSO currentWeapon;

    [SerializeField, ReadOnly] Vector2 velocity = Vector2.zero;
    [SerializeField, ReadOnly] float lastShootTime = 0;

    Controls controls;
    [SerializeField, ReadOnly] Vector2 inputMove = Vector2.zero;
    [SerializeField, ReadOnly] bool inputMoveTo;
    [SerializeField, ReadOnly] bool inputShoot;
    [SerializeField, ReadOnly] bool inputShootHold;

    Rigidbody2D rb;
    Camera cam;
    Plane interactionPlane = new Plane(Vector3.back, Vector3.zero);

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }
    private void OnEnable() {
        controls = new Controls();
        controls.Enable();
        controls.Player.Move.performed += c => inputMove = c.ReadValue<Vector2>();
        controls.Player.Move.canceled += c => inputMove = Vector2.zero;
        controls.Player.MoveTo.performed += c => {
            var pos = c.ReadValue<Vector2>();
            Ray screenRay = cam.ScreenPointToRay(pos);
            if (interactionPlane.Raycast(screenRay, out float enter)) {
                Vector3 point = screenRay.origin + screenRay.direction * enter;
                moveTo.transform.position = point;
            }
            // moveTo.transform.position = cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, -cam.transform.position.z));
            // inputMoveTo = true;
        };
        controls.Player.MoveToPoint.performed += c => inputMoveTo = true;
        controls.Player.MoveToPoint.canceled += c => inputMoveTo = false;
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
    public void SetCurrentWeapon(WeaponSO weapon) {
        Debug.Log($"Switchin player weapon to {weapon.name}");
        currentWeapon = weapon;
    }
    void ShootCurWeapon() {
        // Transform[] curShootPoints = new Transform[currentWeapon.numShootPoints];
        // // todo check
        // switch (curShootPoints.Length) {
        //     case 1:
        //         curShootPoints[0] = shootPoints[0];
        //         break;
        //     case 2:
        //         curShootPoints[0] = shootPoints[1];
        //         curShootPoints[1] = shootPoints[2];
        //         break;
        //     case 3:
        //         curShootPoints[0] = shootPoints[0];
        //         curShootPoints[1] = shootPoints[1];
        //         curShootPoints[2] = shootPoints[2];
        //         break;
        // }
        BulletManager.Instance.Shoot(currentWeapon, shootPoints, true);
        lastShootTime = Time.time;
        inputShoot = false;
    }
    private void FixedUpdate() {
        if (Time.timeScale == 0) return;
        // move
        Vector2 nvel;
        if (inputMoveTo) {
            nvel = moveTo.transform.position - transform.position;
            // inputMoveTo = false;
        } else {
            nvel = inputMove;
        }
        nvel = Vector2.ClampMagnitude(nvel, 1f) * moveSpeed;
        velocity = Vector2.Lerp(velocity, nvel, accelerationRate * Time.deltaTime);
        // if (velocity.sqrMagnitude > 0.001f) {
        // }
        rb.velocity = velocity;

    }
}