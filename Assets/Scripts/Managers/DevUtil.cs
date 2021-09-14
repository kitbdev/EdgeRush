using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DevUtil : MonoBehaviour {

    public bool devMenuOpen = false;
    GameObject player;
    Health playerHealth;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<Health>();
    }
    private void Update() {
        if (Keyboard.current.backquoteKey.wasPressedThisFrame) {
            devMenuOpen = !devMenuOpen;
        }
    }
    private void OnGUI() {
        if (!devMenuOpen) return;
        Rect screenRect = new Rect(0, 50, 120, 500);
        GUILayout.BeginArea(screenRect);
        GUILayout.Label("Dev menu");
        string levellabel = (LevelManager.Instance.curLevel?.title ?? "unknown level") + "\n"
            + (LevelManager.Instance.curLevelEventTitle ?? "unknown level event");
        GUILayout.Label(levellabel);
        playerHealth.manualInvincible = GUILayout.Toggle(playerHealth.manualInvincible, "Invincible");
        for (int i = 0; i < 6; i++) {
            if (GUILayout.Button("Level " + i)) {
                LevelManager.Instance.StartLevel(i);
            }
        }
        GUILayout.EndArea();
    }
}