using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {
    public AudioManager.AudioSettings sound;

    public void PlaySfx() {
        sound.position = transform.position;
        AudioManager.Instance.PlaySfx(sound);
    }
}