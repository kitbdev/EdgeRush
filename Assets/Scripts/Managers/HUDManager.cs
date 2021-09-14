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
    [SerializeField] Slider bossHealthSlider;

    [SerializeField] Health playerHealth;
    [SerializeField] Health curBossHealth;

    protected override void Awake() {
        base.Awake();
    }
    private void OnEnable() {

    }
    private void OnDisable() {

    }

    void UpdatePlayerHealth() {

    }
    void UpdateWeaponAmmoCount() {

    }
    void UpdateCoinCount() {

    }
    void UpdateBossPopup() {
        UpdateBossHealth();

    }
    void UpdateBossHealth() {

    }
}