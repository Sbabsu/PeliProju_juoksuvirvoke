using UnityEngine;

public static class RunResult
{
    public static float lastRunSeconds = 0f;

    public static void CaptureFromGameManager()
    {
        if (GameManager.Instance == null)
        {
            lastRunSeconds = 0f;
            return;
        }

        float start = GameManager.Instance.RunStartRealtime;
        float now = Time.realtimeSinceStartup;
        lastRunSeconds = Mathf.Max(0f, now - start);
    }
}