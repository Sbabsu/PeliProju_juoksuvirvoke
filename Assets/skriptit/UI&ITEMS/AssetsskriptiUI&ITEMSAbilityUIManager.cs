using System.Collections;
using TMPro;
using UnityEngine;

public class AbilityUIManager : MonoBehaviour
{
    public static AbilityUIManager Instance;

    public TextMeshProUGUI abilityText;
    public float defaultDuration = 2f;

    Coroutine routine;

    void Awake()
    {
        Instance = this;
        if (abilityText != null) abilityText.text = "";
    }

    public void Show(string msg, float duration = -1f)
    {
        if (duration < 0f) duration = defaultDuration;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run(msg, duration));
    }

    IEnumerator Run(string msg, float duration)
    {
        abilityText.text = msg;
        yield return new WaitForSeconds(duration);
        abilityText.text = "";
    }
}
