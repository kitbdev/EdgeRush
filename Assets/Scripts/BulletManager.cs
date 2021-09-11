using System.Collections;
using System.Collections.Generic;
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
}