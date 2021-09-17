using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitVisualEffect : MonoBehaviour {

    [SerializeField] Material hitMat;
    [SerializeField] Renderer render;
    [SerializeField] int matIndex = 0;
    [SerializeField] float changeDuration = 0.1f;
    [SerializeField] Health health;

    [SerializeField, ReadOnly] bool isChanged = false;
    [SerializeField, ReadOnly] Material defMat;
    float lastHitTime;

    private void Awake() {
        health ??= GetComponent<Health>();
        defMat = render.sharedMaterials[matIndex];
        isChanged = false;
    }
    private void OnEnable() {
        if (health) {
            health.damageEvent.AddListener(TookDamage);
        }
    }
    private void OnDisable() {
        if (health) {
            health.damageEvent.RemoveListener(TookDamage);
        }
    }
    private void Update() {
        if (isChanged && Time.time > lastHitTime + changeDuration) {
            RestoreMat();
        }
    }
    public void TookDamage() {
        // Debug.Log("hit");
        SetRendererMat(hitMat);
        lastHitTime = Time.time;
        isChanged = true;
    }
    void RestoreMat() {
        // Debug.Log("restore");
        SetRendererMat(defMat);
        isChanged = false;
    }
    void SetRendererMat(Material mat) {
        Material[] mats = render.sharedMaterials;
        mats[matIndex] = mat;
        render.sharedMaterials = mats;
    }
}