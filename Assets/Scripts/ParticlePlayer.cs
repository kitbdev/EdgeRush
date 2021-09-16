using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour {
    public bool play = false;

    ParticleSystem particles;
    private void Awake() {
        particles = GetComponent<ParticleSystem>();
    }
    private void Update() {
        if (!particles.isPlaying) {
            if (play) {
                Play();
            }
        } else {
            if (!play) {
                Stop();
            }
        }
    }
    public void Clear() {
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    public void Stop() {
        particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
    public void Play() {
        if (particles.isPlaying) {
            return;
        }
        particles.Play();
    }
}