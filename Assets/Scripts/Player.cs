using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Player : MonoBehaviour {

    [Header("Movement")]
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float accelerationRate = 2;
    [SerializeField] Transform moveTo;

    [SerializeField, ReadOnly] Transform resetPos;
    [SerializeField, ReadOnly] Vector2 velocity = Vector2.zero;
    [SerializeField, ReadOnly] float lastShootTime = 0;

    [Header("Weapons")]
    [SerializeField] Transform[] shootPoints = new Transform[0];
    [SerializeField] WeaponSO currentWeapon;
    [SerializeField] GameObject[] gunModels = new GameObject[0];

    [Header("Anim")]
    [SerializeField] Transform modelMove;

    [Header("Audio")]
    [SerializeField] AudioClip shootClip;// todo per weapon
    [SerializeField] AudioSource engineAudio;
    [SerializeField] [Range(0, 1)] float minVolume = 0.6f;
    [SerializeField] [Range(0, 1)] float maxVolume = 0.8f;
    [SerializeField] [Range(-3, 3)] float minPitch = 0.9f;
    [SerializeField] [Range(-3, 3)] float maxPitch = 1.2f;

    [Header("Input")]
    [SerializeField, ReadOnly] Vector2 inputMove = Vector2.zero;
    [SerializeField, ReadOnly] bool inputMoveTo;
    [SerializeField, ReadOnly] bool inputShoot;
    [SerializeField, ReadOnly] bool inputShootHold;
    Controls controls;

    Rigidbody2D rb;
    Camera cam;
    Plane interactionPlane = new Plane(Vector3.back, Vector3.zero);
    Health health;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        cam = Camera.main;
        SetCurrentWeapon(currentWeapon);
        resetPos = new GameObject("Player reset pos").transform;
        resetPos.position = transform.position;
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
            // todo switch to moveto mode vs delta mode
            // moveTo.transform.position = cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, -cam.transform.position.z));
            // inputMoveTo = true;
        };
        controls.Player.MoveToPoint.performed += c => inputMoveTo = true;
        controls.Player.MoveToPoint.canceled += c => inputMoveTo = false;
        controls.Player.Fire.performed += c => { inputShootHold = true; inputShoot = true; };
        controls.Player.Fire.canceled += c => { inputShootHold = false; };


        health.dieEvent.AddListener(OnDie);
    }
    private void OnDisable() {
        controls?.Disable();
        health.dieEvent.RemoveListener(OnDie);
    }
    private void Update() {
        if (Time.timeScale == 0) return;
        // input
        if (inputShootHold) {
            if (Time.time > lastShootTime + currentWeapon.shootHoldCooldownDur) {
                ShootCurWeapon();
            }
        } else if (inputShoot) {
            if (Time.time > lastShootTime + currentWeapon.shootCooldownDur) {
                ShootCurWeapon();
            }
        }
        // audio
        float nVol = engineAudio.volume;
        float nPitch = engineAudio.pitch;
        float mag = Mathf.InverseLerp(0f, moveSpeed, rb.velocity.magnitude);
        nPitch = Mathf.Lerp(minPitch, maxPitch, mag);
        nVol = Mathf.Lerp(minVolume, maxVolume, mag);
        if (engineAudio.pitch != nPitch) {
            engineAudio.pitch = nPitch;
        }
        if (engineAudio.volume != nVol) {
            engineAudio.volume = nVol;
        }
    }
    public void SetCurrentWeapon(WeaponSO weapon) {
        Debug.Log($"Switching player weapon to {weapon.name}");
        currentWeapon = weapon;
        for (int i = 0; i < gunModels.Length; i++) {
            GameObject gunModel = gunModels[i];
            if (gunModel != null) gunModel.SetActive(false);
        }
        int weaponIndex = currentWeapon.modelIndex;
        if (weaponIndex >= 0 && weaponIndex < gunModels.Length) {
            gunModels[weaponIndex]?.SetActive(true);
        }
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
        if (shootClip) {
            AudioManager.Instance.PlaySfx(new AudioManager.AudioSettings() {
                clip = shootClip, posOffset = transform.position,
            });
        }
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
    void OnDie() {
        // wait
        // reset level?
        // todo checkpoint
        // LevelManager.Instance.RestartLevel();
        // health.RestoreHealth();
    }
    public void ResetForLevel() {
        // todo ?
        transform.position = resetPos.position;
        rb.velocity = Vector2.zero;
        velocity = Vector2.zero;
    }
    public void ResetAll() {
        // reset position and ammo counts
        ResetForLevel();
        lastShootTime = 0;
        // todo ammo
        // todo checkpoint
    }
}