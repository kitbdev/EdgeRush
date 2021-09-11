using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MenuManager : MonoBehaviour {

    [SerializeField] bool autoFindScreens = false;
    [SerializeField] bool hideAllScreensOnStart = true;
    [SerializeField] List<MenuScreen> allScreens = new List<MenuScreen>();

    public UnityEvent firstShownEvent;
    public UnityEvent allHiddenEvent;

    protected void Awake() {
        if (autoFindScreens) {
            allScreens = GetComponentsInChildren<MenuScreen>().ToList();
        }
    }
    private void Start() {
        if (hideAllScreensOnStart) {
            ForceHideAllScreens();
        }
    }

    public void ShowOnlyScreen(MenuScreen screen) {
        bool isFirst = true;
        allScreens.ForEach(s => {
            if (s != screen) {
                s.Hide();
                isFirst = false;
            }
        });
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