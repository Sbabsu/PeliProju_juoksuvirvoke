using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneDialog : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject dialogRoot;       // esim. paneli
    public TextMeshProUGUI dialogText;

    public void PlayDialog()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        dialogRoot.SetActive(true);

        yield return WaitUntilTime(2.2);
        dialogText.text = "Haloo?";

        yield return WaitUntilTime(4.0);
        dialogText.text = "Joo, mä oon tulossa.";

        yield return WaitUntilTime(6.5);
        dialogText.text = "Nähdään kohta.";

        yield return WaitUntilTime(8.0);
        dialogRoot.SetActive(false);
    }

    IEnumerator WaitUntilTime(double t)
    {
        while (director.time < t)
            yield return null;
    }
}
