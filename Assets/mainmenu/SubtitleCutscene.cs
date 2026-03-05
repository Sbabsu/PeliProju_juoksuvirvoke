using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SubtitleCutscene : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text subtitleText;

    // (Valinnainen) näytetään "Pidä SPACE 2s skippaa" tms.
    [SerializeField] private TMP_Text skipHintText;

    [Header("Next scene")]
    [SerializeField] private string nextSceneName = "ekalevu";

    [Header("Skip settings")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private float holdToSkipTime = 2f;

    [Header("Intro progress (optional)")]
    [SerializeField] private bool markIntroSeen = true;
    [SerializeField] private string introSeenKey = "INTRO_SEEN";

    private Coroutine subtitlesRoutine;
    private bool ending;
    private float holdTimer;

    void Start()
    {
        subtitlesRoutine = StartCoroutine(PlaySubtitles());
        UpdateSkipHint(0f);
    }

    void Update()
    {
        if (ending) return;

        // Jos Game view ei ole fokuksessa, input ei tule -> muista klikata Game-ikkunaa.
        if (Input.GetKey(skipKey))
        {
            holdTimer += Time.unscaledDeltaTime; // toimii vaikka Time.timeScale = 0
            UpdateSkipHint(holdTimer / holdToSkipTime);

            if (holdTimer >= holdToSkipTime)
            {
                StartCoroutine(EndNow());
            }
        }
        else
        {
            if (holdTimer > 0f)
            {
                holdTimer = 0f;
                UpdateSkipHint(0f);
            }
        }
    }

    IEnumerator PlaySubtitles()
    {
        // Alkuviive
        yield return new WaitForSecondsRealtime(5f);

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
        yield return new WaitForSecondsRealtime(4f);

        yield return EndNow();
    }

    IEnumerator ShowLine(string line, float duration)
    {
        if (ending) yield break;

        if (subtitleText != null) subtitleText.text = line;
        yield return new WaitForSecondsRealtime(duration);
    }

    IEnumerator EndNow()
    {
        if (ending) yield break;
        ending = true;

        // pysäytä subtitle-coroutine
        if (subtitlesRoutine != null)
            StopCoroutine(subtitlesRoutine);

        // tyhjennä tekstit
        if (subtitleText != null) subtitleText.text = "";
        if (skipHintText != null) skipHintText.text = "";

        // Merkitse intro nähdyksi (jos haluat)
        if (markIntroSeen)
        {
            PlayerPrefs.SetInt(introSeenKey, 1);
            PlayerPrefs.Save();
        }

        // Fade OUT ennen scenen vaihtoa
        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeOut(0.8f);

        SceneManager.LoadScene(nextSceneName);
    }

    private void UpdateSkipHint(float t01)
    {
        if (skipHintText == null) return;

        t01 = Mathf.Clamp01(t01);
        if (t01 <= 0f)
            skipHintText.text = $"Pidä {skipKey} {holdToSkipTime:0.#}s skippaa";
        else
            skipHintText.text = $"Skippaa... {(t01 * 100f):0}%";
    }
}