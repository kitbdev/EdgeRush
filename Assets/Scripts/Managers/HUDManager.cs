using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : Singleton<HUDManager> {

    [SerializeField] Slider playerHealthSlider;
    [SerializeField] TMP_Text coinCountText;
    [SerializeField] TMP_Text weaponAmmoCountText;

    [SerializeField] GameObject bossPopupGo;
    [SerializeField] TMP_Text bossNameText;
    [SerializeField] Slider bossHealthSlider;

    [SerializeField] Health playerHealth;

    [SerializeField] float healthLerpRate = 10;

    [SerializeField, ReadOnly] Health curBossHealth;
    bool updatingPlayerHealth = false;
    bool updatingBossHealth = false;

    protected override void Awake() {
        base.Awake();
    }
    private void OnEnable() {
        playerHealth?.healthUpdateEvent.AddListener(UpdatePlayerHealth);

        UpdateAll();
    }
    private void OnDisable() {
        playerHealth?.healthUpdateEvent.RemoveListener(UpdatePlayerHealth);
    }
    private void Update() {
        if (updatingPlayerHealth) {
            UpdatePlayerHealth();
        }
        if (updatingBossHealth) {
            UpdateBossHealth();
        }
    }
    public void SetBoss(GameObject bossgo) {
        curBossHealth = bossgo.GetComponent<Health>();
        curBossHealth.healthUpdateEvent.AddListener(UpdateBossHealth);
        curBossHealth.dieEvent.AddListener(BossDie);
        UpdateBossPopup();
    }
    void UpdateAll() {
        UpdatePlayerHealth();
        UpdateCoinCount();
        UpdateWeaponAmmoCount();
        UpdateBossPopup();// includes boss hp
    }
    void UpdatePlayerHealth() {
        float nval = playerHealth.healthPercent;
        // smooth
        float val = Mathf.Lerp(playerHealthSlider.value, nval, Time.deltaTime * healthLerpRate);
        playerHealthSlider.value = val;
        updatingPlayerHealth = val != nval;
    }
    void UpdateWeaponAmmoCount() {

    }
    void UpdateCoinCount() {
        // coinCountText.text = 
    }
    void UpdateBossPopup() {
        if (curBossHealth) {
            // show
            if (bossNameText) bossNameText.text = curBossHealth.gameObject.name;
            UpdateBossHealth();
            bossPopupGo.SetActive(true);
        } else {
            // hide
            bossPopupGo.SetActive(false);
        }
    }
    void BossDie() {
        curBossHealth.healthUpdateEvent.RemoveListener(UpdateBossHealth);
        curBossHealth.dieEvent.RemoveListener(BossDie);
        curBossHealth = null;
        // todo delay?
        UpdateBossPopup();
    }
    void UpdateBossHealth() {
        if (!curBossHealth) {
            // UpdateBossPopup();
            return;
        }
        float nval = curBossHealth.healthPercent;
        // smooth
        float val = Mathf.Lerp(bossHealthSlider.value, nval, Time.deltaTime * healthLerpRate);
        bossHealthSlider.value = val;
        updatingBossHealth = val != nval;
    }
}