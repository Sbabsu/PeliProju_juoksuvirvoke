using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SubtitleCutscene : MonoBehaviour
{
    [SerializeField] private TMP_Text subtitleText;

    void Start()
    {
        StartCoroutine(PlaySubtitles());
    }

    IEnumerator PlaySubtitles()
    {
        // Alkuviive
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

        // Tyhjennetään teksti
        subtitleText.text = "";

        yield return new WaitForSeconds(4f);
        // Fade OUT ennen scenen vaihtoa
        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeOut(0.8f);
        // Ladataan seuraava scene
        SceneManager.LoadScene("ekalevu");
    }

    IEnumerator ShowLine(string line, float duration)
    {
        subtitleText.text = line;
        yield return new WaitForSeconds(duration);
    }
}
