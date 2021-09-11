using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class PauseManager : Singleton<PauseManager> {
    protected override bool destroyIfMultiple => true;

    [ReadOnly] public bool isPaused = false;
    [Min(0)]
    [SerializeField] float timeLerpDur = 0;
    [SerializeField] float minTimeScale = 0f;
    [SerializeField] bool pauseOnStart = false;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] InputActionReference togglePauseButton;
#endif
    IEnumerator pauseLerpCo;

    public UnityEvent pauseEvent;
    public UnityEvent unpauseEvent;

    protected void OnEnable() { 
#if ENABLE_INPUT_SYSTEM
        if (togglePauseButton) {
            togglePauseButton.action.Enable();
            togglePauseButton.action.performed += c => TogglePause();
        }
#endif  
    }
    private void Start() {
        if (pauseOnStart) {
            Pause();
        }
    }

    [ContextMenu("Toggle Pause")]
    public void TogglePause() {
        SetPaused(!isPaused);
    }
    public void Pause() {
        SetPaused(true);
    }
    public void UnPause() {
        SetPaused(false);
    }
    public void SetPaused(bool pause = true) {
        isPaused = pause;
        float targetScale = isPaused ? minTimeScale : 1;
        if (timeLerpDur > 0) {
            StopCoroutine(pauseLerpCo);
            pauseLerpCo = SetTimeScaleCo(targetScale);
            StartCoroutine(pauseLerpCo);
        } else {
            Time.timeScale = targetScale;
        }
        if (isPaused) {
            pauseEvent.Invoke();
        } else {
            unpauseEvent.Invoke();
        }
    }
    IEnumerator SetTimeScaleCo(float target) {
        float initial = Time.timeScale;
        float progress = 0;
        float interp = initial;
        float scaleSpeed = 1f / timeLerpDur;
        while (progress < 1) {
            yield return null;
            progress += Time.unscaledDeltaTime * scaleSpeed;
            interp = Mathf.Lerp(initial, target, progress);
            Time.timeScale = interp;
        }
        Time.timeScale = target;
    }
}