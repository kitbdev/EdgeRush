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
        if (shootPoints.Length != weapon.numShootPoints) {
            Debug.LogWarning("invalid number of shootpoints for " + name);
            return;
        }

        // Debug.Log("Shooting " + name);
        GameObject[] gos = pool.Get(weapon.bulletId, weapon.numShootPoints);
        for (int i = 0; i < weapon.numShootPoints; i++) {
            gos[i].transform.position = shootPoints[i].position;
            gos[i].transform.rotation = shootPoints[i].rotation;
            Vector2 forw = gos[i].transform.up;
            Layer curLayer = isPlayer ? playerBulletLayer : enemyBulletLayer;
            curLayer.SetLayerAllChildren(gos[i]);
            gos[i].GetComponent<Rigidbody2D>().AddForce(forw * weapon.launchForce, ForceMode2D.Impulse);
            gos[i].GetComponentsInChildren<Damager>().ToList().ForEach(d => d.damageAmount = weapon.damage);
        }
    }
}