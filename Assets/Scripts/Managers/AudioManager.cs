using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager> {

    private const string musicVolParam = "VolumeMusic";
    private const string sfxVolParam = "VolumeSfx";

    [SerializeField] [Range(-80, 20)] float maxVol = 10;
    float minVol = -80;
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    private void Start() {
        UpdateSliders();
    }
    public void UpdateSliders() {
        if (musicSlider) musicSlider.SetValueWithoutNotify(GetVolume(musicVolParam));
        if (sfxSlider) sfxSlider.SetValueWithoutNotify(GetVolume(sfxVolParam));
    }
    float NormalizeVolume(float value) {
        return (value - minVol) / (maxVol - minVol);
    }
    float DenormalizeVolume(float value) {
        return value * (maxVol - minVol) + minVol;
    }
    void SetVolume(string paramName, float volumeNorm, bool save = true) {
        volumeNorm = DenormalizeVolume(volumeNorm);
        // Debug.Log($"setting {paramName} to {volumeNorm}");
        // note: this will not work in Awake or OnEnable, Unity bug
        mixer.SetFloat(paramName, volumeNorm);
        if (save) {
            GameManager.Instance.SaveOptionPrefs();
        }
    }
    float GetVolume(string paramName) {
        mixer.GetFloat(paramName, out float val);
        return NormalizeVolume(val);
    }
    // void MuteVolume(string paramName, bool muted) {
    //     mixer.SetFloat(paramName, muted ? -80 : 0);
    // }

    public float GetMusicVolume() {
        return GetVolume(musicVolParam);
    }
    public float GetSfxVolume() {
        return GetVolume(sfxVolParam);
    }
    public void SetMusicVolume(float volume) {
        SetVolume(musicVolParam, volume);
    }
    public void SetSfxVolume(float volume) {
        SetVolume(sfxVolParam, volume);
    }
    public void SetMusicVolumeNoSave(float volume) {
        SetVolume(musicVolParam, volume, false);
    }
    public void SetSfxVolumeNoSave(float volume) {
        SetVolume(sfxVolParam, volume, false);
    }
    // public void MuteMusicVolume(bool muted) {
    //     MuteVolume(musicVolParam, muted);
    // }
    // public void MuteSfxVolume(bool muted) {
    //     MuteVolume(sfxVolParam, muted);
    // }
}