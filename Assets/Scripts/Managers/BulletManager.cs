using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulletManager : Singleton<BulletManager> {

    MultiObjectPool _pool;
    public MultiObjectPool pool => _pool;
    public Layer playerBulletLayer;
    public Layer enemyBulletLayer;

    protected override void Awake() {
        base.Awake();
        _pool = GetComponent<MultiObjectPool>();
    }

    public void Shoot(WeaponSO weapon, Transform shootPoint, bool isPlayer = false) {
        Transform[] shootPoints = new Transform[weapon.numShootPoints];
        for (int i = 0; i < weapon.numShootPoints; i++) {
            shootPoints[i] = shootPoint;
        }
        Shoot(weapon, shootPoints, isPlayer);
    }
    public void Shoot(WeaponSO weapon, Transform[] shootPoints, bool isPlayer = false) {
        if (shootPoints.Length < weapon.numShootPoints) {
            Debug.LogWarning("invalid number of shootpoints for " + name);
            return;
        }

        // Debug.Log("Shooting " + name);
        GameObject[] gos = pool.Get(weapon.bulletId, weapon.numShootPoints);
        for (int i = 0; i < weapon.numShootPoints; i++) {
            GameObject go = gos[i];
            int shootPointIndex = weapon.shotPointIndexes.Length > 0 ? weapon.shotPointIndexes[i] : i;
            go.transform.position = shootPoints[shootPointIndex].position;
            go.transform.rotation = shootPoints[shootPointIndex].rotation;
            go.transform.localScale = weapon.bulletScale * Vector3.one;
            Vector2 forw = go.transform.up;
            Layer curLayer = isPlayer ? playerBulletLayer : enemyBulletLayer;
            curLayer.SetLayerAllChildren(go);
            go.GetComponent<Rigidbody2D>().AddForce(forw * weapon.launchForce, ForceMode2D.Impulse);
            go.GetComponentsInChildren<Damager>().ToList().ForEach(d => d.damageAmount = weapon.damage);
        }
    }
    public void Shoot(BulletSpawnSettings bulletPattern, PatternRunner.SubpatternPlace offset, Transform[] shootPoints, bool isPlayer = false) {
        if (shootPoints.Length < bulletPattern.spawnPointIndices.Length) {
            Debug.LogWarning("invalid number of shootpoints for " + name);
            return;
        }

        // Debug.Log($"Shooting {bulletPattern} {offset}");
        int poolId = BulletManager.Instance.pool.GetTypeId(bulletPattern.prefab);
        GameObject[] gos = pool.Get(poolId, bulletPattern.spawnPointIndices.Length);
        for (int i = 0; i < bulletPattern.spawnPointIndices.Length; i++) {
            GameObject go = gos[i];
            int shootPointIndex = bulletPattern.spawnPointIndices.Length > 0 ? bulletPattern.spawnPointIndices[i] : i;
            go.transform.position = shootPoints[shootPointIndex].position + offset.posWorld;
            go.transform.rotation = shootPoints[shootPointIndex].rotation * offset.angWorld;
            go.transform.localScale = bulletPattern.initScale * Vector3.one;
            Vector2 forw = go.transform.up;
            Layer curLayer = isPlayer ? playerBulletLayer : enemyBulletLayer;
            curLayer.SetLayerAllChildren(go);
            go.GetComponent<Rigidbody2D>().AddForce(forw * bulletPattern.initSpeed, ForceMode2D.Impulse);
            go.GetComponentsInChildren<Damager>().ToList().ForEach(d => d.damageAmount = 1);
            Bullet bullet = go.GetComponent<Bullet>();
            bullet.acceleration = bulletPattern.acceleration;
            bullet.angAcceleration = bulletPattern.angularAcceleration;
            bullet.maxspeed = bulletPattern.maxSpeed;
        }
    }
}