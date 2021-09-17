using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For holding a pool of GameObjects, to reuse them instead of often destroying and instantiating them
/// </summary>
public class MultiObjectPool : MonoBehaviour {

    [Min(0)]
    public int initpoolSizeEach = 0;
    [Min(0)]
    public int maxPoolSizeEach = 100;
    // [Min(0)]
    // public int maxGos = 5000;
    public bool forceAddPoolObjectComponent = false;

    [Space]
    public bool autoDeleteUnusedTypesInPool = false;
    [ConditionalHide(nameof(autoDeleteUnusedTypesInPool), true)]
    [Min(0)]
    public float autoDeleteDur = 10;
    [ConditionalHide(nameof(autoDeleteUnusedTypesInPool), true)]
    [Min(0)]
    public int autoDeletePoolMinSize = 0;

    [ReadOnly] public int outCount = 0;

    [SerializeField] List<GameObject> _prefabs = new List<GameObject>();
    public List<GameObject> prefabs => _prefabs;

    /// <summary>
    /// Called when GameObject is first created, to initialize it
    /// </summary>
    public Action<GameObject, int> creationAction;

    [System.Serializable]
    class GoList : IEnumerable<GameObject> {
        public List<GameObject> poolGos = new List<GameObject>();
        public float lastRecycleTime = 0;

        public GoList(List<GameObject> poolGos) {
            this.poolGos = poolGos;
        }

        public IEnumerator GetEnumerator() {
            return poolGos.GetEnumerator();
        }
        IEnumerator<GameObject> IEnumerable<GameObject>.GetEnumerator() {
            return poolGos.GetEnumerator();
        }

        public static implicit operator List<GameObject>(GoList gl) => gl.poolGos;
        public static implicit operator GoList(List<GameObject> gos) => new GoList(gos);

        internal void Add(GameObject go) {
            poolGos.Add(go);
        }
    }
    [Space]
    // 0 is for no type, 1-numprefabs for prefab
    [SerializeField]
    List<GoList> poolGos = new List<GoList>();
    // Dictionary<int, List<GameObject>> poolGos = new Dictionary<int, List<GameObject>>();// not serialized

    public int currentPoolSize {
        get {
            return poolGos.Sum(gl => gl.poolGos.Count);
        }
    }
    public int totalCount => currentPoolSize + outCount;

    private void Start() {
        Initialize();
    }
    void Initialize() {
        poolGos.Clear();
        poolGos.Add(new List<GameObject>());
        for (int i = 0; i < prefabs.Count; i++) {
            poolGos.Add(new List<GameObject>());
            CreateAmount(i + 1, initpoolSizeEach);
        }
    }
    public void SetPrefabs(List<GameObject> prefabs) {
        this._prefabs = prefabs;
        Initialize();
    }
    private void OnDestroy() {
        // ? clear pool on disable too
        ClearPool();
    }
    private void Update() {
        if (autoDeleteUnusedTypesInPool) {
            foreach (var golist in poolGos) {
                if (golist.poolGos.Count > autoDeletePoolMinSize
                    && Time.time > golist.lastRecycleTime + autoDeleteDur) {
                    // delete excess
                    for (int i = golist.poolGos.Count - 1; i > autoDeletePoolMinSize; i--) {
                        DestroyGo(golist.poolGos[i]);
                        golist.poolGos.RemoveAt(i);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Adds an abount of GameObjects to the pool. 
    /// will not go over max pool size
    /// </summary>
    /// <param name="amount"></param>
    public void CreateAmount(int typeId, int amount) {
        amount = Mathf.Min(amount, maxPoolSizeEach - poolGos[typeId].poolGos.Count);
        if (amount <= 0) return;
        for (int i = 0; i < amount; i++) {
            var go = MakeGo(typeId);
            Recycle(typeId, go);
        }
    }
    public int GetPoolSize(int typeId) {
        return poolGos[typeId].poolGos.Count;
    }
    /// <summary>
    /// Fills the pool up to a certain size of GameObjects. 
    /// Used when expecting lots of need soon
    /// </summary>
    /// <param name="desiredSize"></param>
    public void FillPool(int typeId, int desiredSize) {
        if (desiredSize > currentPoolSize) {
            CreateAmount(typeId, desiredSize - currentPoolSize);
        }
        // ? remove some gos in pool
    }
    /// <summary>
    /// Destroys all GameObjects in the pool
    /// </summary>
    [ContextMenu("Clear pool")]
    public void ClearPool() {
        // delete all
        for (int i = poolGos.Count - 1; i >= 0; i--) {
            foreach (GameObject go in poolGos[i]) {
                DestroyGo(go);
            }
        }
        poolGos.Clear();
    }
    public void ClearType(GameObject prefab) {
        ClearType(GetTypeId(prefab));
    }
    public void ClearType(int typeId) {
        foreach (GameObject go in poolGos[typeId]) {
            DestroyGo(go);
        }
        poolGos[typeId].poolGos.Clear();
    }
    public bool HasPrefab(GameObject prefab) {
        return prefabs.Contains(prefab);
    }
    /// <summary>
    /// Gets the type id for the prefab. 
    /// adds if not found
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public int GetTypeId(GameObject prefab, bool autoAdd = true) {
        if (!prefabs.Contains(prefab)) {
            if (autoAdd) {
                prefabs.Add(prefab);
                poolGos.Add(new List<GameObject>());
                CreateAmount(poolGos.Count - 1, initpoolSizeEach);
            } else {
                return -1;
            }
        }
        // ? hashcode instead?
        var id = prefabs.IndexOf(prefab);
        return id + 1;
    }
    /// <summary>
    /// Gets a number of GameObjects from the pool
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public GameObject[] Get(int typeId, int amount) {
        GameObject[] gos = new GameObject[amount];
        for (int i = 0; i < amount; i++) {
            gos[i] = Get(typeId);
        }
        return gos;
    }
    /// <summary>
    /// Get a GameObject from the pool, 
    /// will activate or create a new one. 
    /// New GameObjects will use prefab if available or make an empty one. 
    /// </summary>
    /// <returns>your new active GameObject</returns>
    public GameObject Get(GameObject prefab) {
        return Get(GetTypeId(prefab));
    }
    /// <summary>
    /// Get a GameObject from the pool, 
    /// will activate or create a new one. 
    /// New GameObjects will use prefab if available or make an empty one. 
    /// </summary>
    /// <returns>your new active GameObject</returns>
    public GameObject Get(int typeId, bool setActive = true) {
        GameObject ngo;
        if (typeId < 0 || typeId > poolGos.Count) {
            // invalid ID!
            Debug.LogWarning($"Trying to get invalid pool typeid {typeId}!");
            return null;
        }
        if (poolGos.Count == 0 || poolGos[typeId].poolGos.Count == 0) {
            ngo = MakeGo(typeId);
        } else {
            List<GameObject> golist = poolGos[typeId];
            ngo = golist[golist.Count - 1];
            golist.RemoveAt(golist.Count - 1);
        }
        if (setActive) ngo.SetActive(true);
        outCount++;
        return ngo;
    }
    /// <summary>
    /// Only recycles if GO has ObjectPoolObject component
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public bool RecyclePoolObject(GameObject go) {
        if (go.TryGetComponent<ObjectPoolObject>(out var poolObject)) {
            Recycle(poolObject.TypeId, go);
            return true;
        }
        return false;
    }
    public void RecyclePoolObject(ObjectPoolObject poolObject) {
        Recycle(poolObject.TypeId, poolObject.gameObject);
    }
    public void Recycle(int typeId, params GameObject[] gos) {
        foreach (var go in gos) {
            Recycle(typeId, go);
        }
    }
    /// <summary>
    /// Remove this GameObject from the scene. 
    /// deactivates and adds to the pool. 
    /// will destroy if pool is full. 
    /// </summary>
    /// <param name="go">GameObject to remove</param>
    public void Recycle(GameObject prefab, GameObject go) {
        Recycle(GetTypeId(prefab), go);
    }
    /// <summary>
    /// Remove this GameObject from the scene. 
    /// deactivates and adds to the pool. 
    /// will destroy if pool is full. 
    /// </summary>
    /// <param name="go">GameObject to remove</param>
    public void Recycle(int typeId, GameObject go) {
        outCount--;
        if (poolGos[typeId].poolGos.Count >= maxPoolSizeEach) {
            DestroyGo(go);
            return;
        }
        go.SetActive(false);
        poolGos[typeId].Add(go);
        poolGos[typeId].lastRecycleTime = Time.time;
    }

    GameObject MakeGo(int typeId = 0) {
        GameObject go;
        int prefabIndex = typeId - 1;
        if (prefabIndex >= 0 && prefabIndex < prefabs.Count) {
            if (prefabs[prefabIndex] == null) {
                Debug.LogWarning("failed to make prefab " + prefabIndex);
                return null;
            }
            go = Instantiate(prefabs[prefabIndex], transform);
        } else {
            go = new GameObject();
            go.transform.SetParent(transform);
        }
        if (go.TryGetComponent<ObjectPoolObject>(out var opgo)) {
            opgo.Init(this, typeId);
        } else if (forceAddPoolObjectComponent) {
            opgo = go.AddComponent<ObjectPoolObject>();
            opgo.Init(this, typeId);
        }
        // first time init Action
        if (creationAction != null) {
            creationAction.Invoke(go, typeId);
        }
        return go;
    }
    void DestroyGo(GameObject go) {
        if (Application.isPlaying) {
            Destroy(go);
        } else {
            DestroyImmediate(go);
        }
    }
}