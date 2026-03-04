using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SubtitleCutscene : MonoBehaviour
{
    [Header("Subtitle")]
    [SerializeField] private TMP_Text subtitleText;

    [Header("Skip UI (optional)")]
    [SerializeField] private TMP_Text skipText; // voit jättää null jos et käytä

    [Header("Next scene")]
    public string nextSceneName = "ekalevu";

    [Header("Skip settings")]
    public KeyCode skipKey = KeyCode.Escape;
    public float holdToSkipTime = 2f;

    [Header("Intro progress")]
    public string introSeenKey = "INTRO_SEEN";

    float skipTimer;
    bool ending;
    Coroutine subtitlesRoutine;

    void Start()
    {
        PlayerPrefs.SetInt(introSeenKey, 1);
        PlayerPrefs.Save();

        UpdateSkipText(0f);

        subtitlesRoutine = StartCoroutine(PlaySubtitles());
    }

    void Update()
    {
        if (ending) return;

        if (Input.GetKey(skipKey))
        {
            skipTimer += Time.deltaTime;
            float t = Mathf.Clamp01(skipTimer / holdToSkipTime);
            UpdateSkipText(t);

            if (skipTimer >= holdToSkipTime)
                StartCoroutine(EndNow());
        }
        else
        {
            if (skipTimer > 0f)
            {
                skipTimer = 0f;
                UpdateSkipText(0f);
            }
        }
    }

    void UpdateSkipText(float normalized)
    {
        if (skipText == null) return;

        int bars = 10;
        int filled = Mathf.RoundToInt(normalized * bars);
        string bar = "[" + new string('■', filled) + new string('□', bars - filled) + "]";

        skipText.text = $"Pidä {skipKey} {holdToSkipTime:0.#}s ohittaaksesi\n{bar}";
    }

    IEnumerator PlaySubtitles()
    {
        yield return new WaitForSeconds(5f);

        yield return ShowLine("morjesta", 1f);
        yield return ShowLine("Noo moro... missä äijä kyntää?", 4f);
        yield return ShowLine("äähh... Päikkareilla olin..", 3f);
        yield return ShowLine("aaha,no haluukos unikeko tulla virvoikkeelle?", 4f);
        yield return ShowLine("ööhh.. Joo, mutta mulla ei oo kaljaa eikä rahaa.", 4f);
        yield return ShowLine("No voi svidy.. kai sä ymmärrät et tää on hätätila?", 4f);
        yield return ShowLine("Tää alkaa joo oleen aika akuutti...", 4f);
        yield return ShowLine("Kaikki olin tähän jo suunnítellu ja tää pilaa kaiken", 4.5f);
        yield return ShowLine("Ei hätää bro, mä hoidan tän", 3f);

        if (subtitleText != null) subtitleText.text = "";
        yield return new WaitForSeconds(4f);

        yield return EndNow();
    }

    IEnumerator EndNow()
    {
        if (ending) yield break;
        ending = true;

        if (subtitlesRoutine != null)
            StopCoroutine(subtitlesRoutine);

        if (subtitleText != null) subtitleText.text = "";
        if (skipText != null) skipText.text = "";

        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeOut(0.8f);

        GameProgress.SaveLastScene(nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator ShowLine(string line, float duration)
    {
        if (ending) yield break;
        if (subtitleText != null) subtitleText.text = line;
        yield return new WaitForSeconds(duration);
    }
}