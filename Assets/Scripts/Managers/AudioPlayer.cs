using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {

    public AudioManager.AudioSettings sound;
    public float minTimeBetween = 0.1f;
    float lastPlayTime;
    public bool ignoreIfPaused = false;
    public bool dontPlay = false;

    public void PlaySfx() {
        if (dontPlay) {
            return;
        }
        if (ignoreIfPaused && Time.timeScale == 0) {
            return;
        }
        if (Time.unscaledTime > lastPlayTime + minTimeBetween) {
            lastPlayTime = Time.unscaledTime;
            sound.position = transform.position;
            AudioManager.Instance.PlaySfx(sound);
        }
    }
}