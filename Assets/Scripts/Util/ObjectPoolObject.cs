using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Not necessary for ObjectPool, just a helper in case other scripts want to remove
/// </summary>
public class ObjectPoolObject : MonoBehaviour {

    ObjectPool pool;
    int typeId;
    MultiObjectPool multiObjectPool;

    public int TypeId => typeId;
    public void Init(ObjectPool pool) {
        this.pool = pool;
    }
    public void Init(MultiObjectPool pool, int typeId) {
        this.multiObjectPool = pool;
        this.typeId = typeId;
    }
    [ContextMenu("Recycle From Pool")]
    public void RecycleFromPool() {
        if (pool != null) {
            pool.Recycle(gameObject);
        } else if (multiObjectPool != null) {
            multiObjectPool.Recycle(typeId, gameObject);
        }
    }
}