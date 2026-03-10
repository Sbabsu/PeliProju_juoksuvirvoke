using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float RunStartRealtime { get; private set; }

    const string VSYNC_KEY = "vsync";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RunStartRealtime = Time.realtimeSinceStartup;

        ApplySavedVsync();
    }

    void OnEnable()
    {
        RunStartRealtime = Time.realtimeSinceStartup;
    }

    void ApplySavedVsync()
    {
        bool vsync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1;

        if (vsync)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 144;
        }
    }

    public bool IsExitEnabled() => true;
}