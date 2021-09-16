using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

    public struct OptionPrefs {
        public float musicVol;
        public float sfxVol;
        public bool fullscreen;
        // public bool invertY;
        public override string ToString() {
            return $"mv:{musicVol} sv:{sfxVol} f:{fullscreen}";
        }
    }
    const string savekey = "EdgeRushPreferences";

    [SerializeField] MenuManager menuManager;
    [SerializeField] MenuScreen mainMenuScreen;
    [SerializeField] MenuScreen winScreen;
    [SerializeField] MenuScreen loseScreen;
    [SerializeField] bool mainMenuOnStart = true;
    [SerializeField] bool editorMainMenuOnStart = true;

    bool isFullScreen = true;

    private void Start() {
        TryLoadOptionPrefs();
#if UNITY_EDITOR
        if (editorMainMenuOnStart) {
#else
        if (mainMenuOnStart) {
#endif
            BackToMainMenu();
        }
    }
    public void SaveOptionPrefs() {
        var savesettings = new OptionPrefs() {
            musicVol = AudioManager.Instance.GetMusicVolume(),
            sfxVol = AudioManager.Instance.GetSfxVolume(),
            fullscreen = isFullScreen,
        };
        string jsonsave = JsonUtility.ToJson(savesettings);
        PlayerPrefs.SetString(savekey, jsonsave);
        PlayerPrefs.Save();
        Debug.Log("saved prefs!");
    }
    public void TryLoadOptionPrefs() {
        if (PlayerPrefs.HasKey(savekey)) {
            string savestr = PlayerPrefs.GetString(savekey);
            var loadedoptions = JsonUtility.FromJson<OptionPrefs>(savestr);
            AudioManager.Instance.SetMusicVolumeNoSave(loadedoptions.musicVol);
            AudioManager.Instance.SetSfxVolumeNoSave(loadedoptions.sfxVol);
            AudioManager.Instance.UpdateSliders();
            SetFullScreen(loadedoptions.fullscreen, false);
            Debug.Log("Loaded prefs! " + loadedoptions);
        }
    }

    void SetFullScreen(bool enabled, bool save = true) {
        isFullScreen = enabled;
        if (isFullScreen) {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        } else {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        if (save) {
            SaveOptionPrefs();
        }
    }
    public void ToggleFullscreen() {
        SetFullScreen(!isFullScreen);
    }

    [ContextMenu("Main menu")]
    public void BackToMainMenu() {
        PauseManager.Instance.Pause();
        LevelManager.Instance.StopGame();
        menuManager.ShowOnlyScreen(mainMenuScreen);
    }
    [ContextMenu("win")]
    public void PlayerWin() {
        PauseManager.Instance.Pause();
        // LevelManager.Instance.StopGame();
        menuManager.ShowOnlyScreen(winScreen);
    }
    [ContextMenu("lose")]
    public void PlayerLose() {
        PauseManager.Instance.Pause();
        // LevelManager.Instance.StopGame();
        menuManager.ShowOnlyScreen(loseScreen);
    }
}