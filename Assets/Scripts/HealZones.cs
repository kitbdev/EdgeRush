using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class HealZones : MonoBehaviour {

    [SerializeField] float healRate = 1;
    [SerializeField] float activeDuration = 5;
    [SerializeField] GameObject[] gameObjects = new GameObject[0];
    [SerializeField, ReadOnly] bool isActive;
    float activateTime = 0;
    Health playerHealth;

    private void Awake() {
        Deactivate();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
    }
    private void Update() {
        if (isActive) {
            if (Time.time > activateTime + activeDuration) {
                Deactivate();
            }
        }
    }
    public bool CanActivate() {
        return true;
    }
    public void Activate() {
        if (!CanActivate()) {
            return;
        }
        activateTime = Time.time;
        isActive = true;
        foreach (var go in gameObjects) {
            go.SetActive(true);
        }
    }
    public void Deactivate() {
        isActive = false;
        // todo animation
        foreach (var go in gameObjects) {
            go.SetActive(false);
        }
    }
    public void Heal() {
        // Debug.Log("healing player");
        playerHealth.Heal(healRate * Time.deltaTime);
    }
}