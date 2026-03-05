using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CapRatingUI : MonoBehaviour
{
    [Header("Caps (5 images)")]
    [SerializeField] private Image[] capImages;   // Cap1..cap5
    [SerializeField] private Sprite capFilled;

    [Header("Drop Animation")]
    [SerializeField] private float dropHeight = 60f;
    [SerializeField] private float dropTime = 0.18f;
    [SerializeField] private float settleTime = 0.10f;
    [SerializeField] private float delayBetweenCaps = 0.08f;

    [SerializeField] private float popScale = 1.15f;
    [SerializeField] private float wobbleDegrees = 7f;

    private Coroutine animRoutine;

    void Awake()
    {
        HideAllCaps();
    }

    public void HideRow()
    {
        StopAnim();
        HideAllCaps();
        gameObject.SetActive(false);
    }

    public void ShowRow()
    {
        gameObject.SetActive(true);
        // pakota layout päivittämään paikat heti
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void SetDropCount(int caps)
    {
        caps = Mathf.Clamp(caps, 0, 5);

        for (int i = 0; i < capImages.Length; i++)
        {
            if (!capImages[i]) continue;

            bool active = i < caps;
            capImages[i].gameObject.SetActive(active);

            if (active)
            {
                capImages[i].sprite = capFilled;
                capImages[i].rectTransform.localScale = Vector3.one;
                capImages[i].rectTransform.localRotation = Quaternion.identity;

                // näkyy mutta alpha 0 aluksi
                SetAlpha(capImages[i], 0f);
            }
        }

        // LayoutGroup asettaa oikeat paikat
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void PlayDrop(int caps)
    {
        caps = Mathf.Clamp(caps, 0, 5);
        StopAnim();
        animRoutine = StartCoroutine(DropRoutine(caps));
    }

    void StopAnim()
    {
        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }
    }

    void HideAllCaps()
    {
        if (capImages == null) return;
        for (int i = 0; i < capImages.Length; i++)
        {
            if (!capImages[i]) continue;
            capImages[i].gameObject.SetActive(false);
        }
    }

    IEnumerator DropRoutine(int caps)
    {
        // varmistetaan, että layout-paikat ovat oikein ennen animaatiota
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

        for (int i = 0; i < caps && i < capImages.Length; i++)
        {
            var img = capImages[i];
            if (!img) continue;

            yield return DropOne(img.rectTransform, img);
            yield return new WaitForSecondsRealtime(delayBetweenCaps);
        }
    }

    IEnumerator DropOne(RectTransform rt, Image img)
    {
        // Ota target position siitä missä layout on sen jo asettanut
        Vector2 target = rt.anchoredPosition;
        Vector2 start = target + Vector2.up * dropHeight;

        rt.anchoredPosition = start;

        // drop + fade in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.001f, dropTime);
            float e = EaseOut(t);

            rt.anchoredPosition = Vector2.Lerp(start, target, e);
            SetAlpha(img, e);

            yield return null;
        }

        // settle/pop
        float dir = (Random.value < 0.5f) ? -1f : 1f;
        Quaternion startRot = Quaternion.identity;
        Quaternion peakRot = Quaternion.Euler(0, 0, wobbleDegrees * dir);

        Vector3 startScale = Vector3.one;
        Vector3 peakScale = Vector3.one * popScale;

        float s = 0f;
        while (s < 1f)
        {
            s += Time.unscaledDeltaTime / Mathf.Max(0.001f, settleTime);
            float e = EaseOut(s);

            rt.localRotation = Quaternion.Slerp(startRot, peakRot, e);
            rt.localScale = Vector3.Lerp(startScale, peakScale, e);

            yield return null;
        }

        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
        rt.anchoredPosition = target;
        SetAlpha(img, 1f);
    }

    void SetAlpha(Image img, float a)
    {
        if (!img) return;
        var c = img.color;
        c.a = Mathf.Clamp01(a);
        img.color = c;
    }

    static float EaseOut(float x)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, 3f);
    }
}