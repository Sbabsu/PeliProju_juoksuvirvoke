using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SettingsPanelTransition : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform panel; // jos tyhjä -> käyttää tätä GameObjectia
    public CanvasGroup canvasGroup; // jos tyhjä -> GetComponent

    [Header("Motion")]
    public Vector2 hiddenOffset = new Vector2(0f, -900f); // mistä suunnasta tulee (alas)
    public float duration = 0.25f;

    [Header("Behaviour")]
    public bool startHidden = true;

    Vector2 shownPos;
    Coroutine routine;
    bool isOpen;

    void Awake()
    {
        if (panel == null) panel = transform as RectTransform;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        shownPos = panel.anchoredPosition;

        if (startHidden)
        {
            panel.anchoredPosition = shownPos + hiddenOffset;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            isOpen = false;
            gameObject.SetActive(false); // pidetään pois päältä kun kiinni
        }
        else
        {
            isOpen = true;
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        gameObject.SetActive(true);

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Animate(open: true));
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Animate(open: false));
    }

    IEnumerator Animate(bool open)
    {
        Vector2 fromPos = panel.anchoredPosition;
        Vector2 toPos = open ? shownPos : (shownPos + hiddenOffset);

        float fromA = canvasGroup.alpha;
        float toA = open ? 1f : 0f;

        // klikkaukset päälle vasta lopussa
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration);
            float k = Mathf.SmoothStep(0f, 1f, t);

            panel.anchoredPosition = Vector2.Lerp(fromPos, toPos, k);
            canvasGroup.alpha = Mathf.Lerp(fromA, toA, k);

            yield return null;
        }

        panel.anchoredPosition = toPos;
        canvasGroup.alpha = toA;

        if (open)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            gameObject.SetActive(false);
        }

        routine = null;
    }
}