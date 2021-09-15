using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : Singleton<HUDManager> {

    [SerializeField] Slider playerHealthSlider;
    [SerializeField] TMP_Text coinCountText;
    [SerializeField] TMP_Text weaponAmmoCountText;
    [SerializeField] GameObject weaponUIPrefab;
    [SerializeField] Transform weaponUIParent;

    [SerializeField] GameObject bossPopupGo;
    [SerializeField] TMP_Text bossNameText;
    [SerializeField] Slider bossHealthSlider;

    [Space]
    [SerializeField] Player player;
    [SerializeField] Health playerHealth;

    [SerializeField] float healthLerpRate = 10;

    [SerializeField, ReadOnly] Health curBossHealth;
    bool updatingPlayerHealth = false;
    bool updatingBossHealth = false;

    protected override void Awake() {
        base.Awake();
    }
    private void OnEnable() {
        if (player) player.coinAmountChangeEvent += UpdateCoinCount;
        if (player) player.weaponAmmoChangeEvent += UpdateWeaponAmmoCount;
        playerHealth?.healthUpdateEvent.AddListener(UpdatePlayerHealth);

        UpdateAll();
    }
    private void OnDisable() {
        if (player) player.coinAmountChangeEvent -= UpdateCoinCount;
        if (player) player.weaponAmmoChangeEvent -= UpdateWeaponAmmoCount;
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
    [ContextMenu("Update UI")]
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
        int numWeaponUis = weaponUIParent.childCount;
        int wantedWeaponNum = player.weaponDatas.Count;
        Debug.Log("updating weaponui " + numWeaponUis + " " + wantedWeaponNum);
        List<WeaponUI> weaponUIs = new List<WeaponUI>();
        if (numWeaponUis != wantedWeaponNum) {
            // erase all and remake
            for (int i = 0; i < numWeaponUis; i++) {
                // erase
                GameObject wUi = weaponUIParent.GetChild(i).gameObject;
                if (Application.isPlaying) {
                    Destroy(wUi);
                } else {
                    DestroyImmediate(wUi);
                }
            }
            for (int i = 0; i < wantedWeaponNum; i++) {
                var wUi = Instantiate(weaponUIPrefab, weaponUIParent);
                weaponUIs.Add(wUi.GetComponent<WeaponUI>());
                // position happens autmatically
            }
            // ! need to wait a frame for transform to update
        } else {
            for (int i = 0; i < wantedWeaponNum; i++) {
                Transform wUi = weaponUIParent.GetChild(i);
                weaponUIs.Add(wUi.GetComponent<WeaponUI>());
            }
        }
        // update texts
        for (int i = 0; i < weaponUIs.Count; i++) {
            WeaponUI weaponUI = weaponUIs[i];
            Player.WeaponData weaponData = player.weaponDatas[i];
            if (weaponData.weaponType.hasUnlimitedAmmo) {
                weaponUI.ammoCountText.text = "infinite";
            } else {
                weaponUI.ammoCountText.text = weaponData.ammoAmount + "";
            }
            weaponUI.weaponNameText.text = weaponData.weaponType.name;
            if (weaponData == player.currentWeaponData) {
                weaponUI.weaponSelectedGo.SetActive(true);
            } else {
                weaponUI.weaponSelectedGo.SetActive(false);
            }
        }
    }
    void UpdateCoinCount() {
        coinCountText.text = player.numCoins + "";
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