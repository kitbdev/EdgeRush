using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Bullet : MonoBehaviour {

    public float angularSpeed;
    public float acceleration;
    public float angularAcceleration;
    public float maxspeed;

    [ReadOnly] public float initAngle;
    [ReadOnly] public float initSpeed;
    [ReadOnly] public Quaternion initRot;

    [ReadOnly] public float speed = 0;
    [ReadOnly] public float angle = 0;
    [ReadOnly] public Vector2 velocity;

    [ReadOnly] public float timeoutDur = 0.5f;
    [ReadOnly] public float enableTime = 0;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public ObjectPoolObject objectPoolObject;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        objectPoolObject = GetComponent<ObjectPoolObject>();
    }

    public void Init() {
        enableTime = Time.time;
        speed = initSpeed;
        angle = initAngle;
        initRot = transform.localRotation;
    }
}
[System.Serializable]
public class BulletSpawnSettings {
    public GameObject prefab;
    public float initSpeed = 2;
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