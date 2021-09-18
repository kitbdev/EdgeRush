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
    [SerializeField] bool loadPrefs = true;
    [SerializeField] AudioPlayer[] pauseAudioPlayers = new AudioPlayer[0];
#pragma warning disable 0219
    [SerializeField] GameObject[] buildWEBRemoveGos = new GameObject[0];
#pragma warning restore 0219

    bool isFullScreen = true;

    protected override void Awake() {
        base.Awake();
#if UNITY_WEBGL
        foreach (var item in buildWEBRemoveGos) {
            item.SetActive(false);
        }
#endif
    }
    private void Start() {
        if (loadPrefs) {
            TryLoadOptionPrefs();
        }
        if (mainMenuOnStart) {
            ShowMainMenu();
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

    public void HideMenu() {
        PauseManager.Instance.UnPause();
        menuManager.HideAllScreens();
        SetAudioPlayers(false);
    }
    [ContextMenu("Main menu")]
    void ShowMainMenu() {
        SetAudioPlayers(true);
        PauseManager.Instance.Pause();
        LevelManager.Instance.StopGame();
        menuManager.ShowOnlyScreen(mainMenuScreen);
        PauseManager.Instance.blockPause = true;
    }

    public void SetAudioPlayers(bool stop) {
        foreach (AudioPlayer audioPlayer in pauseAudioPlayers) {
            audioPlayer.ignoreIfPaused = stop;
        }
    }

    public void BackToMainMenu() {
        DG.Tweening.DOTween.KillAll();
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
    [ContextMenu("win")]
    public void PlayerWin() {
        SetAudioPlayers(true);
        PauseManager.Instance.Pause();
        PauseManager.Instance.blockPause = true;
        menuManager.ShowOnlyScreen(winScreen);
    }
    [ContextMenu("lose")]
    public void PlayerLose() {
        SetAudioPlayers(true);
        PauseManager.Instance.Pause();
        PauseManager.Instance.blockPause = true;
        menuManager.ShowOnlyScreen(loseScreen);
    }
    public void RetryLastLevel() {
        PauseManager.Instance.blockPause = false;
        PauseManager.Instance.UnPause();
        SetAudioPlayers(false);
        LevelManager.Instance.RetryLevel();
        menuManager.HideAllScreens();
    }
    public void RestartLastLevel() {
        PauseManager.Instance.blockPause = false;
        PauseManager.Instance.UnPause();
        SetAudioPlayers(false);
        LevelManager.Instance.RestartLevel();
        menuManager.HideAllScreens();
    }

    public void ExitGame() {
        // todo save?
#if !UNITY_WEBGL
        Application.Quit();
#endif
    }
}