using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SubtitleCutscene : MonoBehaviour
{
    [System.Serializable]
    public class SubtitleLine
    {
        public string speaker;
        [TextArea(2, 4)] public string text;
        public float duration = 3f;
        public AudioClip voiceClip;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text skipHintText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Dialogue")]
    [SerializeField] private SubtitleLine[] lines;
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float endDelay = 2f;

    [Header("Next scene")]
    [SerializeField] private string nextSceneName = "ekalevu";

    [Header("Skip settings")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private float holdToSkipTime = 2f;

    [Header("Intro progress")]
    [SerializeField] private bool markIntroSeen = true;
    [SerializeField] private string introSeenKey = "INTRO_SEEN";

    private Coroutine subtitlesRoutine;
    private bool ending;
    private float holdTimer;

    private void Start()
    {
        subtitlesRoutine = StartCoroutine(PlaySubtitles());
        UpdateSkipHint(0f);
    }

    private void Update()
    {
        if (ending) return;

        if (Input.GetKey(skipKey))
        {
            holdTimer += Time.unscaledDeltaTime;
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

    private IEnumerator PlaySubtitles()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        foreach (SubtitleLine line in lines)
        {
            yield return ShowLine(line);
        }

        if (subtitleText != null)
            subtitleText.text = "";

        yield return new WaitForSecondsRealtime(endDelay);

        yield return EndNow();
    }

    private IEnumerator ShowLine(SubtitleLine line)
    {
        if (ending) yield break;

        if (subtitleText != null)
        {
            if (!string.IsNullOrWhiteSpace(line.speaker))
                subtitleText.text = $"{line.speaker}: {line.text}";
            else
                subtitleText.text = line.text;
        }

        if (audioSource != null && line.voiceClip != null)
        {
            audioSource.clip = line.voiceClip;
            audioSource.Play();

            // Jos haluat että kesto tulee automaattisesti audion mukaan:
            float waitTime = line.voiceClip.length > 0f ? line.voiceClip.length : line.duration;
            yield return new WaitForSecondsRealtime(waitTime);
        }
        else
        {
            yield return new WaitForSecondsRealtime(line.duration);
        }
    }

    private IEnumerator EndNow()
    {
        if (ending) yield break;
        ending = true;

        if (subtitlesRoutine != null)
            StopCoroutine(subtitlesRoutine);

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        if (subtitleText != null) subtitleText.text = "";
        if (skipHintText != null) skipHintText.text = "";

        if (markIntroSeen)
        {
            PlayerPrefs.SetInt(introSeenKey, 1);
            PlayerPrefs.Save();
        }

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