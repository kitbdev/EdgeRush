using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulletManager : Singleton<BulletManager> {

    MultiObjectPool _pool;
    public MultiObjectPool pool => _pool;
    public Layer playerBulletLayer;
    public Layer enemyBulletLayer;
    public int maxTotalBullets = 6000;
    public List<Bullet> activeBullets = new List<Bullet>();

    protected override void Awake() {
        base.Awake();
        _pool = GetComponent<MultiObjectPool>();
    }
    private void Update() {
        float curTime = Time.time;
        for (int i = 0; i < activeBullets.Count; i++) {
            Bullet bullet = activeBullets[i];
            if (curTime > bullet.enableTime + bullet.timeoutDur) {
                RemoveBullet(i);
                i--;
            }
        }
    }
    private void FixedUpdate() {
        foreach (var bullet in activeBullets) {
            // physics
            if (bullet.acceleration != 0) {
                bullet.speed += bullet.acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime;
            }
            if (bullet.angularSpeed != 0) {
                bullet.angularSpeed += bullet.angularAcceleration * Time.fixedDeltaTime * Time.fixedDeltaTime;
                bullet.angle += bullet.angularSpeed * Time.fixedDeltaTime;
                // bullet.transform.up = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            }
            bullet.transform.localRotation = bullet.initRot * Quaternion.Euler(0, 0, Mathf.Rad2Deg * bullet.angle);
            Vector2 vel = bullet.transform.up * bullet.speed;
            // bullet.rb.velocity = vel;
            bullet.rb.MovePosition(bullet.rb.position + vel * Time.fixedDeltaTime);
            // bullet.velocity = vel;
            // bullet.transform.position = bullet.transform.position + (Vector3)bullet.velocity * Time.deltaTime;
        }
    }
    public void ClearAllActiveBullets() {
        for (int i = activeBullets.Count - 1; i >= 0; i--) {
            Debug.Assert(activeBullets[i] != null);
            RemoveBullet(i);
        }
        activeBullets.Clear();
    }
    public void RemoveBullet(int index) {
        Bullet bullet = activeBullets[index];
        activeBullets.RemoveAt(index);
        pool.RecyclePoolObject(bullet.objectPoolObject);
    }
    public void RemoveBullet(Bullet bullet) {
        activeBullets.Remove(bullet);
        pool.RecyclePoolObject(bullet.objectPoolObject);
    }
    void ClearOldestBullets(int amount) {
        int clearamount = Mathf.Min(amount, activeBullets.Count);
        for (int i = 0; i < clearamount; i++) {
            RemoveBullet(activeBullets[0]);
        }
    }
    public void Shoot(WeaponSO weapon, Transform shootPoint, bool isPlayer = false) {
        int numBullets = weapon.bulletSpawnSettings.spawnPointIndices.Length;
        if (numBullets == 0) numBullets = 1;
        Transform[] shootPoints = new Transform[numBullets];
        for (int i = 0; i < numBullets; i++) {
            shootPoints[i] = shootPoint;
        }
        Shoot(weapon, shootPoints, isPlayer);
    }
    public void Shoot(WeaponSO weapon, Transform[] shootPoints, bool isPlayer = false) {
        Shoot(weapon.bulletSpawnSettings, PatternRunner.BulletInitState.origin, shootPoints, isPlayer);
        // if (shootPoints.Length < weapon.numShootPoints) {
        //     Debug.LogWarning("invalid number of shootpoints for " + name);
        //     return;
        // }

        // // Debug.Log("Shooting " + name);
        // GameObject[] gos = pool.Get(weapon.bulletId, weapon.numShootPoints);
        // for (int i = 0; i < weapon.numShootPoints; i++) {
        //     GameObject go = gos[i];
        //     int shootPointIndex = weapon.shotPointIndexes.Length > 0 ? weapon.shotPointIndexes[i] : i;
        //     go.transform.position = shootPoints[shootPointIndex].position;
        //     go.transform.rotation = shootPoints[shootPointIndex].rotation;
        //     go.transform.localScale = weapon.bulletScale * Vector3.one;
        //     Vector2 forw = go.transform.up;
        //     Layer curLayer = isPlayer ? playerBulletLayer : enemyBulletLayer;
        //     curLayer.SetLayerAllChildren(go);
        //     go.GetComponent<Rigidbody2D>().AddForce(forw * weapon.launchForce, ForceMode2D.Impulse);
        //     go.GetComponentsInChildren<Damager>().ToList().ForEach(d => d.damageAmount = weapon.damage);
        // }
    }
    public void Shoot(BulletSpawnSettings bulletPattern, PatternRunner.BulletInitState initState, Transform[] shootPoints, bool isPlayer = false) {
        int numBullets = bulletPattern.spawnPointIndices.Length > 0 ? bulletPattern.spawnPointIndices.Length : 1;
        if (shootPoints.Length < numBullets) {
            Debug.LogWarning("invalid number of shootpoints for " + name);
            return;
        }

        // Debug.Log($"Shooting {bulletPattern} {offset}");
        if (activeBullets.Count + numBullets >= maxTotalBullets) {
            ClearOldestBullets(numBullets);
        }
        int poolId = pool.GetTypeId(bulletPattern.prefab);
        GameObject[] gos = pool.Get(poolId, numBullets);
        for (int i = 0; i < numBullets; i++) {
            GameObject go = gos[i];
            int shootPointIndex = bulletPattern.spawnPointIndices.Length > 0 ? bulletPattern.spawnPointIndices[i] : i;
            go.transform.position = shootPoints[shootPointIndex].position + initState.posWorld;
            go.transform.rotation = shootPoints[shootPointIndex].rotation * initState.angWorld;
            go.transform.localScale = bulletPattern.initScale * Vector3.one;
            Vector2 forw = go.transform.up;
            Layer curLayer = isPlayer ? playerBulletLayer : enemyBulletLayer;
            if (go.layer != curLayer) {
                curLayer.SetLayerAllChildren(go);
            }
            float initSpeed = bulletPattern.initSpeed + initState.speed;
            // go.GetComponent<Rigidbody2D>().AddForce(forw * initSpeed, ForceMode2D.Impulse);
            // go.GetComponent<Damager>().damageAmount = 1;
            Bullet bullet = go.GetComponent<Bullet>();
            bullet.acceleration = bulletPattern.acceleration;// todo add to initstate
            bullet.angularAcceleration = bulletPattern.angularAcceleration;
            bullet.maxspeed = bulletPattern.maxSpeed;
            bullet.initSpeed = initSpeed;
            bullet.angularSpeed = initState.angSpeed;
            bullet.acceleration = initState.acceleration;
            bullet.angularAcceleration = initState.angAcceleration;
            activeBullets.Add(bullet);
            bullet.Init();
        }
    }
}