using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public Text pauseText;

    private bool paused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        paused = !paused;

        if (paused)
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);

            pauseText.text =
                "TAUKOKUITTI\n\n" +
                "AIKA: 00:21\n" +
                "MATKA: 0.1 KM\n\n" +
                "KERÄTTY:\n" +
                "NALLE III SPURDO   1x\n" +
                "HEINÄKENKÄ        1x\n\n" +
                "HUMALA: 33%\n" +
                "KUNTO: 78%";
        }
        else
        {
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
        }
    }
}
