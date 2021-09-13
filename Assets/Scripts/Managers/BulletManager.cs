using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulletManager : Singleton<BulletManager> {

    MultiObjectPool _pool;
    public MultiObjectPool pool => _pool;
    public Layer playerBulletLayer;
    public Layer enemyBulletLayer;
    public List<Bullet> activeBullets = new List<Bullet>();

    protected override void Awake() {
        base.Awake();
        _pool = GetComponent<MultiObjectPool>();
    }

    private void FixedUpdate() {
        foreach (var bullet in activeBullets) {
            // physics
        }
    }
    public void ClearAllActiveBullets() {
        for (int i = activeBullets.Count - 1; i >= 0; i--) {
            RemoveBullet(activeBullets[i]);
        }
        activeBullets.Clear();
    }
    public void RemoveBullet(Bullet bullet) {
        activeBullets.Remove(bullet);
        pool.RecyclePoolObject(bullet.gameObject);
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
            go.GetComponent<Damager>().damageAmount = 1;
            Bullet bullet = go.GetComponent<Bullet>();
            bullet.acceleration = bulletPattern.acceleration;
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