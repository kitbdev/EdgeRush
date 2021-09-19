using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour {
    public GameObject prefab;

    public void SpawnPrefab() {
        if (prefab) {
            Instantiate(prefab, transform.position, transform.rotation);
        }
    }
}