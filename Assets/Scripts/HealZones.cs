﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class HealZones : MonoBehaviour {

    [SerializeField] float healRate = 1;
    [SerializeField] float activeDuration = 5;
    [SerializeField] GameObject[] gameObjects = new GameObject[0];
    [SerializeField] Animator[] animators = new Animator[0];
    [SerializeField] AudioSource healingAudio;
    [SerializeField] AudioSource jetsAudio;
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
        SetActive(true);
    }
    public void Deactivate() {
        isActive = false;
        SetActive(false);
        StopHeal();
    }
    void SetActive(bool enabled) {
        isActive = true;
        if (jetsAudio) {
            if (enabled) {
                jetsAudio.Play();
            } else {
                jetsAudio.Stop();
            }
        }
        foreach (var go in gameObjects) {
            go.SetActive(enabled);
        }
        foreach (var anim in animators) {
            anim.SetBool("Play", enabled);
        }
    }
    public void Heal() {
        // Debug.Log("healing player");
        playerHealth.Heal(healRate * Time.deltaTime);
        if (healingAudio && !healingAudio.isPlaying) {
            healingAudio.Play();
        }
    }
    public void StopHeal() {
        if (healingAudio && healingAudio.isPlaying) {
            healingAudio.Stop();
        }
    }
}