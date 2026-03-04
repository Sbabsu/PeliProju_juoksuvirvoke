using UnityEngine;

public static class GameProgress
{
    // Kirjoita 6 kenttää järjestyksessä
    public static readonly string[] Levels =
    {
        "ekalevu",
        "Tokalevu",
        "kartta3",
        "kartta4",
        "kartta5",
        "kartta6",
    };

    private const string KEY_HAS_SAVE = "HAS_SAVE";
    private const string KEY_NEXT_INDEX = "NEXT_LEVEL_INDEX";
    private const string KEY_LAST_SCENE = "LAST_SCENE";

    public static bool HasSave() => PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;

    public static int GetNextLevelIndex()
    {
        if (Levels == null || Levels.Length == 0) return 0;
        return Mathf.Clamp(PlayerPrefs.GetInt(KEY_NEXT_INDEX, 0), 0, Levels.Length - 1);
    }

    public static string GetNextLevelScene()
    {
        if (Levels == null || Levels.Length == 0) return "";
        return Levels[GetNextLevelIndex()];
    }

    // ✅ “Jatka” tulee näkyviin vasta kun eka kenttä on läpäisty
    public static bool IsContinueUnlocked()
    {
        return HasSave() && GetNextLevelIndex() >= 1;
    }

    public static void SaveLastScene(string sceneName)
    {
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.SetString(KEY_LAST_SCENE, sceneName);
        PlayerPrefs.Save();
    }

    public static void MarkLevelCompleted(string completedScene)
    {
        if (Levels == null || Levels.Length == 0) return;

        int idx = System.Array.IndexOf(Levels, completedScene);
        if (idx < 0) return;

        int next = Mathf.Clamp(idx + 1, 0, Levels.Length - 1);

        int currentSaved = PlayerPrefs.GetInt(KEY_NEXT_INDEX, 0);
        if (next > currentSaved)
            PlayerPrefs.SetInt(KEY_NEXT_INDEX, next);

        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KEY_HAS_SAVE);
        PlayerPrefs.DeleteKey(KEY_NEXT_INDEX);
        PlayerPrefs.DeleteKey(KEY_LAST_SCENE);
        PlayerPrefs.Save();
    }
}