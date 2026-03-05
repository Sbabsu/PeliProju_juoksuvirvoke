using System.Collections;
using UnityEngine;

public class SettingsPanelTransition : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform panel; // jos tyhjä -> käyttää tätä GameObjectia

    [Header("Motion")]
    public Vector2 hiddenOffset = new Vector2(0f, -900f);
    public float duration = 0.25f;

    [Header("Behaviour")]
    public bool startHidden = true;

    Vector2 shownPos;
    Coroutine routine;
    bool isOpen;

    void Awake()
    {
        if (panel == null)
            panel = transform as RectTransform;

        shownPos = panel.anchoredPosition;

        if (startHidden)
        {
            panel.anchoredPosition = shownPos + hiddenOffset;
            isOpen = false;
            gameObject.SetActive(false);
        }
        else
        {
            isOpen = true;
        }
    }

    public void Open()
    {
        if (isOpen) return;

        isOpen = true;
        gameObject.SetActive(true);

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Animate(true));
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Animate(false));
    }

    IEnumerator Animate(bool open)
    {
        Vector2 fromPos = panel.anchoredPosition;
        Vector2 toPos = open ? shownPos : shownPos + hiddenOffset;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration);
            float k = Mathf.SmoothStep(0f, 1f, t);

            panel.anchoredPosition = Vector2.Lerp(fromPos, toPos, k);

            yield return null;
        }

        panel.anchoredPosition = toPos;

        if (!open)
            gameObject.SetActive(false);

        routine = null;
    }
}