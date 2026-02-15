using UnityEngine;

public class RunTimer : MonoBehaviour
{
    public static RunTimer Instance { get; private set; }

    public float RunStartRealtime { get; private set; }
    public bool IsRunning { get; private set; }
    public bool WasCaught { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartRun()
    {
        RunStartRealtime = Time.realtimeSinceStartup;
        IsRunning = true;
        WasCaught = false;
    }

    public void MarkCaught()
    {
        WasCaught = true;
        IsRunning = false;
    }

    public void MarkCompleted()
    {
        IsRunning = false;
    }

    public float GetElapsedSeconds()
    {
        if (RunStartRealtime <= 0f) return 0f;
        return Time.realtimeSinceStartup - RunStartRealtime;
    }
}
