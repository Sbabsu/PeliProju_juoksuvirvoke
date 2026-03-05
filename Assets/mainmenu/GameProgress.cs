using UnityEngine;

public static class GameProgress
{
    // --- PlayerPrefs keys ---
    public const string KEY_LAST_SCENE = "LAST_SCENE";
    public const string KEY_FIRST_LEVEL_DONE = "FIRST_LEVEL_DONE";
    public const string KEY_INTRO_SEEN = "INTRO_SEEN";

    // ⚠️ MUOKKAA NÄMÄ VASTAAMAAN TEIDÄN SCENE-NIMIÄ (Build Settingsissä!)
    // 6 karttaa järjestyksessä:
    public static readonly string[] LevelOrder =
    {
        "ekalevu",
        "Tokalevu",
        "kartta3",
        "kartta4",
        "kartta5",
        "kartta6"
    };

    // Missä mainmenu/cutscene ovat (ei tallenneta jatkopaikaksi)
    public static readonly string[] NonGameplayScenes =
    {
        "MainMenu",
        "MainMenu_CutScene",
        "Cutscene"
    };

    // --- Save / load last scene ---
    public static void SaveCurrentScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (IsNonGameplay(sceneName)) return;

        PlayerPrefs.SetString(KEY_LAST_SCENE, sceneName);
        PlayerPrefs.Save();
    }

    // Vanhoja nimiä varten (ettei tule CS0117 jos joku kutsuu näitä)
    public static void SaveLastScene(string sceneName) => SaveCurrentScene(sceneName);
    public static void SaveCurrentSceneName(string sceneName) => SaveCurrentScene(sceneName);

    public static string GetLastScene()
    {
        return PlayerPrefs.GetString(KEY_LAST_SCENE, "");
    }

    public static bool HasLastScene()
    {
        var s = GetLastScene();
        return !string.IsNullOrEmpty(s) && !IsNonGameplay(s);
    }

    // --- Continue unlock logic ---
    // “Jatka” näkyy vasta kun 1. kartta on pelattu / läpäisty
    public static void MarkFirstLevelDone()
    {
        PlayerPrefs.SetInt(KEY_FIRST_LEVEL_DONE, 1);
        PlayerPrefs.Save();
    }

    public static bool IsContinueUnlocked()
    {
        return PlayerPrefs.GetInt(KEY_FIRST_LEVEL_DONE, 0) == 1 && HasLastScene();
    }

    // --- Intro seen ---
    public static bool IsIntroSeen() => PlayerPrefs.GetInt(KEY_INTRO_SEEN, 0) == 1;
    public static void MarkIntroSeen()
    {
        PlayerPrefs.SetInt(KEY_INTRO_SEEN, 1);
        PlayerPrefs.Save();
    }

    // --- Level progression ---
    public static string GetNextLevelScene(string currentScene)
    {
        int idx = IndexOf(LevelOrder, currentScene);
        if (idx < 0) return LevelOrder.Length > 0 ? LevelOrder[0] : "";
        int next = idx + 1;
        if (next >= LevelOrder.Length) return ""; // ei enää seuraavaa
        return LevelOrder[next];
    }

    // Kutsu tätä kun level läpäistään:
    // - merkitsee ekan kartan suoritetuksi jos se oli ekalevu
    // - tallentaa seuraavan jatkopaikaksi
    public static string OnLevelCompleted(string currentScene)
    {
        if (LevelOrder.Length > 0 && currentScene == LevelOrder[0])
            MarkFirstLevelDone();

        string next = GetNextLevelScene(currentScene);
        if (!string.IsNullOrEmpty(next))
            SaveCurrentScene(next);

        return next;
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_SCENE);
        PlayerPrefs.DeleteKey(KEY_FIRST_LEVEL_DONE);
        PlayerPrefs.DeleteKey(KEY_INTRO_SEEN);
        PlayerPrefs.Save();
    }

    public static void ClearSave() => ResetProgress(); // alias

    static bool IsNonGameplay(string sceneName)
    {
        foreach (var s in NonGameplayScenes)
            if (s == sceneName) return true;
        return false;
    }

    static int IndexOf(string[] arr, string value)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == value) return i;
        return -1;
    }
}