using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Bullet : MonoBehaviour {

    public float initAngle;
    public float initSpeed;
    public float angularSpeed;
    public float acceleration;
    public float angularAcceleration;
    public Quaternion initRot;

    public float maxspeed;

    [SerializeField, ReadOnly] public float speed = 0;
    [SerializeField, ReadOnly] public float angle = 0;
    [ReadOnly] public Vector2 velocity;


    [SerializeField] public float timeoutDur = 0.5f;
    [ReadOnly] public float enableTime = 0;


    public Rigidbody2D rb;
    [ReadOnly] public int activeIndex = -1;
    [ReadOnly] public ObjectPoolObject objectPoolObject;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        objectPoolObject = GetComponent<ObjectPoolObject>();
    }
    private void OnEnable() {
        // enableTime = Time.time;
        // initAngle = transform.localRotation;
    }
    public void Init() {
        enableTime = Time.time;
        speed = initSpeed;
        angle = initAngle;
        initRot = transform.localRotation;
    }
    // private void FixedUpdate() {
    //     // todo move all logic (including physics) elsewhere
    //     if (acceleration != 0) {
    //         speed += acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime;
    //     }
    //     if (angularSpeed != 0) {
    //         angularSpeed += angularAcceleration * Time.fixedDeltaTime * Time.fixedDeltaTime;
    //         angle += angularSpeed * Time.fixedDeltaTime;
    //         // transform.up = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
    //     }
    //     transform.localRotation = initRot * Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
    //     rb.velocity = transform.up * speed;
    // }
}
[System.Serializable]
public class BulletSpawnSettings {
    public GameObject prefab;
    public float initSpeed = 5;
    public float initAngle = 0;
    public float acceleration = 0;
    public float maxSpeed = 10;
    public float angularAcceleration = 0;
    public float maxAngularSpeed = 10;
    public float initScale = 1;
    public int[] spawnPointIndices = new int[0];

    public override string ToString() {
        return base.ToString() + " " +
        initSpeed + ", " +
        initAngle + ", " +
        acceleration + ", " +
        maxSpeed + ", " +
        angularAcceleration + ", " +
        maxAngularSpeed + ", " +
        initScale + ", " +
        spawnPointIndices.Length + ", ";
    }
}