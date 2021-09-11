using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple Timer. Triggers an event after some time
/// </summary>
public class EventTimer : MonoBehaviour
{

    public float duration = 5;
    public bool readyOnStart = true;
    public bool autoRepeat = false;
    public bool useUnscaledTimeScale = false;

    [SerializeField, ReadOnly] protected bool running = false;
    [SerializeField, ReadOnly] protected float timer = 0;

    public UnityEvent callEvent;

    public float timerValue => timer;

    protected void Start()
    {
        if (readyOnStart)
        {
            StartTimer();
        }
    }
    protected void Update()
    {
        if (running)
        {
            if (timer >= duration)
            {
                callEvent.Invoke();
                StopTimer();
                if (autoRepeat)
                {
                    StartTimer();
                }
            } else
            {
                timer += useUnscaledTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            }
        }
    }
    [ContextMenu("Stop Timer")]
    public void StopTimer()
    {
        running = false;
        timer = 0;
    }
    [ContextMenu("Start Timer")]
    public void StartTimer()
    {
        timer = 0;
        running = true;
    }
    public void PauseTimer(bool paused = true)
    {
        running = !paused;
    }
    public void RestartTimer()
    {
        StopTimer();
        StartTimer();
    }
}