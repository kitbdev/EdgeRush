using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For holding a pool of GameObjects, to reuse them instead of often destroying and instantiating them
/// </summary>
public class ObjectPool : MonoBehaviour
{

    [Min(0)]
    public int initpoolSize = 0;
    [Min(0)]
    public int maxPoolSize = 100;
    /// <summary>
    /// Optional prefab instantiation
    /// </summary>
    public GameObject prefab;

    /// <summary>
    /// Called when GameObject is first created, to initialize it
    /// </summary>
    public Action<GameObject> creationAction;

    // todo get action or remove action necessary?
    // just use onenable or disable on the go
    ///// <summary>
    ///// called when GameObject is gotten from the pool
    ///// </summary>
    //// public Action<GameObject> getAction;
    ///// <summary>
    ///// called when GameObject is put back into the pool
    ///// </summary>
    //// public Action<GameObject> removeAction;

    [Space]
    [SerializeField, ReadOnly]
    List<GameObject> poolGos = new List<GameObject>();

    public int currentPoolSize => poolGos.Count;

    private void Awake()
    {
        Initialize();
    }
    void Initialize()
    {
        CreateAmount(initpoolSize);
    }
    private void OnDestroy()
    {
        // ? clear pool on disable too
        ClearPool();
    }
    /// <summary>
    /// Adds an abount of GameObjects to the pool. 
    /// will not go over max pool size
    /// </summary>
    /// <param name="amount"></param>
    public void CreateAmount(int amount)
    {
        amount = Mathf.Min(amount, maxPoolSize - currentPoolSize);
        if (amount <= 0) return;
        for (int i = 0; i < amount; i++)
        {
            var go = MakeGo();
            Recycle(go);
        }
    }
    /// <summary>
    /// Fills the pool up to a certain size of GameObjects. 
    /// Used when expecting lots of need soon
    /// </summary>
    /// <param name="desiredSize"></param>
    public void FillPool(int desiredSize)
    {
        if (desiredSize > currentPoolSize)
        {
            CreateAmount(desiredSize - currentPoolSize);
        }
        // ? remove some gos in pool
    }
    /// <summary>
    /// Destroys all GameObjects in the pool
    /// </summary>
    [ContextMenu("Clear pool")]
    public void ClearPool()
    {
        // delete all
        for (int i = poolGos.Count - 1; i >= 0; i--)
        {
            DestroyGo(poolGos[i]);
        }
        poolGos.Clear();
    }
    /// <summary>
    /// Gets a number of GameObjects from the pool
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public GameObject[] Get(int amount)
    {
        GameObject[] gos = new GameObject[amount];
        for (int i = 0; i < amount; i++)
        {
            gos[i] = Get();
        }
        return gos;
    }
    /// <summary>
    /// Get a GameObject from the pool, 
    /// will activate or create a new one. 
    /// New GameObjects will use prefab if available or make an empty one. 
    /// </summary>
    /// <returns>your new active GameObject</returns>
    public GameObject Get()
    {
        GameObject ngo;
        if (poolGos.Count == 0)
        {
            ngo = MakeGo();
        } else
        {
            ngo = poolGos[poolGos.Count - 1];
            poolGos.RemoveAt(poolGos.Count - 1);
        }
        // ? dont activate yet?
        ngo.SetActive(true);
        // if (getAction != null)
        // {
        //     getAction.Invoke(ngo);
        // }
        return ngo;
    }
    /// <summary>
    /// Remove this GameObject from the scene. 
    /// deactivates and adds to the pool. 
    /// will destroy if pool is full. 
    /// </summary>
    /// <param name="go">GameObject to remove</param>
    public void Recycle(GameObject go)
    {
        // ? delayed remove option
        // if (removeAction != null)
        // {
        //     removeAction.Invoke(go);
        // }
        if (poolGos.Count >= maxPoolSize)
        {
            DestroyGo(go);
            return;
        }
        // todo try to maintain certain pool size?
        go.SetActive(false);
        poolGos.Add(go);
    }

    GameObject MakeGo()
    {
        GameObject go;
        if (prefab)
        {
            go = Instantiate(prefab, transform);
        } else
        {
            go = new GameObject();
            go.transform.SetParent(transform);
        }
        if (go.TryGetComponent<ObjectPoolObject>(out var opgo))
        {
            opgo.Init(this);
        }
        // first time init Action
        if (creationAction != null)
        {
            creationAction.Invoke(go);
        }
        return go;
    }
    void DestroyGo(GameObject go)
    {
        if (Application.isPlaying)
        {
            Destroy(go);
        } else
        {
            DestroyImmediate(go);
        }
    }
}