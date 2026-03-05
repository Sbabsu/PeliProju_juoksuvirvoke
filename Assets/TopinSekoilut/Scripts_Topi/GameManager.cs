using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float RunStartRealtime { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        RunStartRealtime = Time.realtimeSinceStartup;
    }

    void OnEnable()
    {
        // Ensures start time is correct if object is re-enabled
        RunStartRealtime = Time.realtimeSinceStartup;
    }

    private void Start()
    {
        Application.targetFrameRate = 144;
    }

    public bool IsExitEnabled() => true;
}