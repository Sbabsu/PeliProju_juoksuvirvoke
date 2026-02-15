using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float defaultDuration = 0.8f;

    Coroutine running;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // jos unohdit vetää inspectorissa
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        // aloita läpinäkyvänä
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        if (canvasGroup == null) yield break;
        if (duration <= 0f) duration = defaultDuration;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeTo(1f, duration));
        yield return running;
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        if (canvasGroup == null) yield break;
        if (duration <= 0f) duration = defaultDuration;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeTo(0f, duration));
        yield return running;
    }

    IEnumerator FadeTo(float target, float duration)
    {
        canvasGroup.blocksRaycasts = true;

        float start = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // toimii myös jos timeScale=0
            float a = Mathf.Lerp(start, target, t / duration);
            canvasGroup.alpha = a;
            yield return null;
        }

        canvasGroup.alpha = target;
        canvasGroup.blocksRaycasts = target > 0.01f;
    }
}
