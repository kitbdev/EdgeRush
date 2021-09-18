using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class Player : MonoBehaviour {

    [System.Serializable]
    public class WeaponData {
        public WeaponSO weaponType;
        public int ammoAmount;
        public bool isUnlocked = false;
    }

    [Header("Movement")]
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float accelerationRate = 2;
    [SerializeField] Transform moveTo;
    [SerializeField] float playAreaWidth = 10;

    [SerializeField, ReadOnly] Transform resetPos;
    [SerializeField, ReadOnly] Vector2 velocity = Vector2.zero;

    [Header("Weapons")]
    [SerializeField] Transform[] shootPoints = new Transform[0];
    [SerializeField] GameObject[] gunModels = new GameObject[0];
    [SerializeField] WeaponSO initialWeapon;
    public bool debugUnlimitedShots = false;
    [SerializeField, ReadOnly] public List<WeaponData> weaponDatas = new List<WeaponData>();
    [SerializeField, ReadOnly] public int curSelectedWeapon = 0;
    [SerializeField, ReadOnly] float lastShootTime = 0;
    [SerializeField, ReadOnly] float lastTryShootTime = 0;

    public WeaponData currentWeaponData =>
        (curSelectedWeapon >= 0 && curSelectedWeapon < weaponDatas.Count) ? weaponDatas[curSelectedWeapon] : null;
    public WeaponSO currentWeapon => currentWeaponData?.weaponType ?? null;

    [Header("Anim")]
    [SerializeField] Transform modelMove;
    [SerializeField] float modelTiltX = 20;
    [SerializeField] float modelTiltY = 10;

    [Header("Misc")]
    [SerializeField] int healthZoneCoinCost = 10;
    [SerializeField] int startCoinAmount = 0;
    [SerializeField] HealZones healZone;
    [SerializeField, ReadOnly] public int numCoins;

    [Header("Audio")]
    [SerializeField] AudioManager.AudioSettings noAmmoSfx;
    [SerializeField] AudioManager.AudioSettings damageSfx;
    [SerializeField] AudioManager.AudioSettings[] deadSfx = new AudioManager.AudioSettings[0];
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
    [SerializeField, ReadOnly] bool mouseInBounds;
    Controls controls;

    public event System.Action weaponAmmoChangeEvent;
    public event System.Action coinAmountChangeEvent;

    Rigidbody2D rb;
    Camera cam;
    Plane interactionPlane = new Plane(Vector3.back, Vector3.zero);
    Health health;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        cam = Camera.main;
        resetPos = new GameObject("Player reset pos").transform;
        resetPos.position = transform.position;
    }
    private void OnEnable() {
        inputMoveTo = false;
        controls = new Controls();
        controls.Enable();
        controls.Player.Move.performed += c => {
            inputMove = c.ReadValue<Vector2>();
            inputMoveTo = false;
        };
        controls.Player.Move.canceled += c => inputMove = Vector2.zero;
        controls.Player.MoveTo.performed += c => {
            var pos = c.ReadValue<Vector2>();
            Ray screenRay = cam.ScreenPointToRay(pos);
            if (interactionPlane.Raycast(screenRay, out float enter)) {
                mouseInBounds = false;
                Vector3 point = screenRay.origin + screenRay.direction * enter;
                if (point.x >= -playAreaWidth && point.x <= playAreaWidth
                    && point.y >= -playAreaWidth && point.y <= playAreaWidth) {
                    // is inbounds 
                    mouseInBounds = true;
                    moveTo.transform.position = point;
                }
                // if (!mouseInBounds) {
                //     inputMoveTo = false;
                // }
            }
        };
        controls.Player.MoveToPoint.performed += c => {
            // touch down
            if (mouseInBounds) {
                inputMoveTo = true;
            }
        };
        controls.Player.MoveToPoint.canceled += c => {
            // inputMoveTo = false;
        };
        controls.Player.Fire.performed += c => {
            if (Time.timeScale == 0) return;
            inputShootHold = true;
            inputShoot = true;
        };
        controls.Player.Fire.canceled += c => { inputShootHold = false; };
        controls.Player.FireMoveTo.performed += c => {
            if (Time.timeScale == 0) return;
            // test click pos
            if (mouseInBounds) {
                inputShoot = true;
                inputShootHold = true;
            }
        };
        controls.Player.FireMoveTo.canceled += c => {
            // if (mouseInBounds) {
            inputShootHold = false;
            // }
        };
        controls.Player.SelectWeaponNext.performed += c => {
            SelectWeaponNext();
        };
        controls.Player.SelectWeaponPrev.performed += c => { SelectWeaponPrev(); };
        controls.Player.SelectWeaponScroll.performed += c => {
            if (c.ReadValue<float>() < 0) {
                SelectWeaponNext();
            } else {
                SelectWeaponPrev();
            }
        };
        controls.Player.SelectWeapon1.performed += c => { SelectWeapon(0); };
        controls.Player.SelectWeapon2.performed += c => { SelectWeapon(1); };
        controls.Player.SelectWeapon3.performed += c => { SelectWeapon(2); };
        controls.Player.SelectWeapon4.performed += c => { SelectWeapon(3); };
        controls.Player.EnableHealZones.performed += c => { TryActivateHealthZones(); };

        health.dieEvent.AddListener(OnDie);
        health.damageEvent.AddListener(OnDamaged);
    }
    private void OnDisable() {
        controls?.Disable();
        health.dieEvent.RemoveListener(OnDie);
        health.damageEvent.RemoveListener(OnDamaged);
    }
    private void Update() {
        if (Time.timeScale == 0) {
            if (engineAudio.isPlaying) {
                engineAudio.Stop();
            }
            return;
        }
        // input
        if (currentWeapon != null) {
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
        // audio
        if (!engineAudio.isPlaying) {
            engineAudio.Play();
        }
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
    void OnDamaged() {
        if (damageSfx != null) {
            damageSfx.position = transform.position;
            AudioManager.Instance.PlaySfx(damageSfx);
        }
    }
    void SelectWeaponNext() {
        int index = curSelectedWeapon + 1;
        if (index >= weaponDatas.Count) {
            index = 0;
        }
        SelectWeapon(index);
    }
    void SelectWeaponPrev() {
        int index = curSelectedWeapon - 1;
        if (index < 0) {
            index = weaponDatas.Count - 1;
        }
        SelectWeapon(index);
    }
    public void SelectWeapon(int index) {
        if (Time.timeScale == 0) return;
        if (index < 0 || index >= weaponDatas.Count) {
            // invalid index
            return;
        }
        curSelectedWeapon = index;
        Debug.Log($"Switching player weapon to {currentWeapon.name}");
        // update model
        for (int i = 0; i < gunModels.Length; i++) {
            GameObject gunModel = gunModels[i];
            if (gunModel != null) gunModel.SetActive(false);
        }
        int weaponIndex = currentWeapon.modelIndex;
        if (weaponIndex >= 0 && weaponIndex < gunModels.Length) {
            gunModels[weaponIndex]?.SetActive(true);
        }
        weaponAmmoChangeEvent?.Invoke();
    }
    WeaponData SetCurrentWeapon(WeaponSO weapon, int ammoToAdd = 0, bool andSelect = true) {
        // adds if not found
        int index = -1;
        for (int i = 0; i < weaponDatas.Count; i++) {
            if (weaponDatas[i].weaponType == weapon) {
                index = i;
                break;
            }
        }
        if (index >= 0) {
            weaponDatas[index].ammoAmount += ammoToAdd;
            if (andSelect) { SelectWeapon(index); }
            weaponAmmoChangeEvent?.Invoke();
            return weaponDatas[index];
        } else {
            // make new weapon data
            WeaponData weaponData = new WeaponData();
            weaponData.weaponType = weapon;
            weaponData.ammoAmount = ammoToAdd;
            weaponDatas.Add(weaponData);
            if (andSelect) { SelectWeapon(weaponDatas.Count - 1); }
            weaponAmmoChangeEvent?.Invoke();
            return weaponData;
        }
    }
    public void PickupWeaponAmmo(WeaponSO weaponType, int ammo) {
        SetCurrentWeapon(weaponType, ammo, false);
    }
    public void AddCoins(int amount) {
        numCoins += amount;
        coinAmountChangeEvent?.Invoke();
    }
    public void TryActivateHealthZones() {
        if (Time.timeScale == 0) return;
        if (numCoins >= healthZoneCoinCost && healZone.CanActivate()) {
            healZone.Activate();
            numCoins -= healthZoneCoinCost;
            coinAmountChangeEvent?.Invoke();
        }
    }
    void ShootCurWeapon() {
        if (!currentWeapon) {
            return;
        }
        inputShoot = false;
        if (!debugUnlimitedShots) {
            if (!(currentWeapon.hasUnlimitedAmmo || currentWeaponData.ammoAmount > 0)) {
                // not enough ammo!
                if (Time.time > lastTryShootTime + currentWeapon.shootHoldCooldownDur) {
                    noAmmoSfx.position = transform.position;
                    AudioManager.Instance.PlaySfx(noAmmoSfx);
                    lastTryShootTime = Time.time;
                }
                return;
            }
        }
        BulletManager.Instance.Shoot(currentWeapon, shootPoints, true);
        lastShootTime = Time.time;
        currentWeapon.shootAudio.position = transform.position;
        AudioManager.Instance.PlaySfx(currentWeapon.shootAudio);
        currentWeaponData.ammoAmount -= 1;
        weaponAmmoChangeEvent?.Invoke();
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
        float tiltx = -velocity.x / moveSpeed * modelTiltX;
        float tilty = velocity.y / moveSpeed * modelTiltY;
        Quaternion modelTilt = Quaternion.Euler(tilty, tiltx, 0);
        modelMove.rotation = modelTilt;
        // if (velocity.sqrMagnitude > 0.001f) {
        // }
        rb.velocity = velocity;
    }
    public void AddKnockback(Vector2 amount) {
        velocity += amount;
    }
    void OnDie() {
        // todo wait?
        int rindex = Random.Range(0,deadSfx.Length);
        deadSfx[rindex].position = transform.position;
        AudioManager.Instance.PlaySfx(deadSfx[rindex]);
        GameManager.Instance.PlayerLose();
    }
    public void ResetForLevel() {
        // weaponAmmoChangeEvent?.Invoke();
    }
    public void ResetAll() {
        // reset position and ammo counts
        ResetForLevel();
        transform.position = resetPos.position;
        rb.velocity = Vector2.zero;
        velocity = Vector2.zero;

        health.RestoreHealth();
        lastShootTime = 0;
        numCoins = startCoinAmount;
        weaponDatas.Clear();
        curSelectedWeapon = 0;
        if (initialWeapon) {
            SetCurrentWeapon(initialWeapon);
        }
        weaponAmmoChangeEvent?.Invoke();
        coinAmountChangeEvent?.Invoke();
    }
}