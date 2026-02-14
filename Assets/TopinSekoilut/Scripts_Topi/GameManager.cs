using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ✅ tämä toimii aina, vaikka UI olisi disabloitu
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

    private void Start()
    {
        Application.targetFrameRate = 144;
    }

    // Exitille pääsee aina
    public bool IsExitEnabled() => true;
}
