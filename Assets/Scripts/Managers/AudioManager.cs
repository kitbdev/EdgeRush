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
    [Space]
    public AudioMixerGroup defaultGroup;
    [SerializeField] GameObject audioPrefab;
    [SerializeField] ObjectPool objectPool;


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

    [System.Serializable]
    public class AudioSettings {
        public AudioClip clip;
        public Transform follow;
        public Vector3 posOffset;
        public AudioMixerGroup group;
        [Range(0, 1)]
        public float volume = 1;
        [Range(-3, 3)]
        public float pitch = 1;
        [Range(-1, 1)]
        public float pan = 0f;
        [Range(0, 256)]
        public int priority = 128;
    }
    public void PlaySfx(AudioClip clip) {
        PlaySfx(new AudioSettings() {
            clip = clip,
        });
    }
    public void PlaySfx(AudioSettings audioSettings) {
        if (!audioSettings.clip) return;
        GameObject audioGo;
        if (objectPool != null) {
            audioGo = objectPool.Get();
        } else {
            if (audioPrefab != null) {
                audioGo = Instantiate(audioPrefab);
            } else {
                audioGo = new GameObject();
                audioGo.AddComponent<AudioSource>();
            }
        }

        if (audioSettings.follow) {
            audioGo.transform.parent = audioSettings.follow;
        } else {
            audioGo.transform.parent = transform;
        }
        audioGo.transform.localPosition = audioSettings.posOffset;
        var source = audioGo.GetComponent<AudioSource>();
        source.clip = audioSettings.clip;
        source.outputAudioMixerGroup = audioSettings.group ?? defaultGroup ?? source.outputAudioMixerGroup;
        source.volume = audioSettings.volume;
        source.pitch = audioSettings.pitch;
        source.panStereo = audioSettings.pan;
        source.priority = audioSettings.priority;
        // todo? change over time
        source.Play();
        StartCoroutine(RemoveFromPool(audioGo, audioSettings.clip.length));
    }
    IEnumerator RemoveFromPool(GameObject go, float dur) {
        yield return new WaitForSeconds(dur);
        if (objectPool == null) {
            Destroy(go);
        } else {
            objectPool.Recycle(go);
        }
    }
}