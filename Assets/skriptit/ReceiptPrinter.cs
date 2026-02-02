using System.Collections;
using TMPro;
using UnityEngine;

public class ReceiptPrinter : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text bodyTMP;

    [Header("Print settings")]
    [Tooltip("Sekunteina per rivi (esim 0.02f = nopea)")]
    public float lineDelay = 0.02f;

    [Tooltip("Halutessasi ääniefekti per rivi/merkki")]
    public AudioSource printSound;

    Coroutine co;

    // Tämä on se metodi jota ReceiptUI_TMP yrittää kutsua
    public void PrintTo(TMP_Text target, string fullText)
    {
        if (target == null) return;

        // jos inspectorissa bodyTMP on asetettu, käytä sitä jos target puuttuu
        if (target == null && bodyTMP != null) target = bodyTMP;

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(PrintRoutine(target, fullText));
    }

    IEnumerator PrintRoutine(TMP_Text target, string fullText)
    {
        target.text = "";

        // tulosta rivi kerrallaan (luonnollinen kuittifiilis)
        var lines = fullText.Replace("\r\n", "\n").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            target.text += lines[i] + "\n";

            if (printSound != null) printSound.Play();

            if (lineDelay > 0f)
                yield return new WaitForSecondsRealtime(lineDelay); // toimii Time.timeScale = 0 aikana
            else
                yield return null;
        }
    }
}
