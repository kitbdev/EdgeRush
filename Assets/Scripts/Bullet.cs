using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public float speed;
    public float ang;
    public float angSpeed;

    public float acceleration;
    public float angAcceleration;
    public float maxspeed;
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