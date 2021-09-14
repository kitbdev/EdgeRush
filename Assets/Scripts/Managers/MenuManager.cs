using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour {

    [SerializeField] bool autoFindScreens = false;
    [SerializeField] bool hideAllScreensOnStart = true;
    [SerializeField] List<MenuScreen> allScreens = new List<MenuScreen>();

    [SerializeField, ReadOnly] MenuScreen currentScreen;
    [SerializeField, ReadOnly] MenuScreen lastScreen;

    [Header("Events")]
    public UnityEvent firstShownEvent;
    public UnityEvent allHiddenEvent;

    protected void Awake() {
        if (autoFindScreens) {
            allScreens = GetComponentsInChildren<MenuScreen>().ToList();
        }
    }
    private void OnEnable() {
        if (hideAllScreensOnStart) {
            ForceHideAllScreens();
        }
    }
    // private void Start() {
    // }

    public void SwitchToLastScreen() {
        ShowOnlyScreen(lastScreen);
    }
    public void ShowOnlyScreen(MenuScreen screen) {
        bool isFirst = true;
        allScreens.ForEach(s => {
            if (s != screen) {
                s.Hide();
                isFirst = false;
            }
        });
        lastScreen = currentScreen;
        currentScreen = screen;
        screen.Show();
        if (isFirst) {
            firstShownEvent.Invoke();
        }
    }
    public void ShowScreen(MenuScreen screen) {
        bool isFirst = !allScreens.Any(s => s.isShown);
        screen.Show();
        if (isFirst) {
            firstShownEvent.Invoke();
        }
    }
    public void HideScreen(MenuScreen screen) {
        screen.Hide();
        allHiddenEvent.Invoke();
    }
    public void ForceHideAllScreens() {
        allScreens.ForEach(s => {
            s.ForceSetVisible(false);
        });
        allHiddenEvent.Invoke();
    }
    public void HideAllScreens() {
        allScreens.ForEach(s => {
            s.Hide();
        });
        allHiddenEvent.Invoke();
    }


    public void ExitGame() {
        // todo save!
        Debug.LogWarning("todo save");
        ExitGameNoSave();
    }
    public void ExitGameNoSave() {
        Application.Quit();
    }
}