using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;     // tämä on se pullo/pause-menu root
    public Button resumeButton;
    public Button exitButton;

    [Header("Exit target")]
    public string exitSceneName = "MainMenu";

    [Header("Options")]
    public KeyCode toggleKey = KeyCode.Escape;

    [Header("Slide")]
    public float slideSpeed = 800f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip wooshClip;
    public AudioClip bottleBreakClip;

    bool isPaused;
    bool isAnimating;

    RectTransform panelRect;

    Vector2 centerPos;
    Vector2 topPos;
    Vector2 bottomPos;

    void Awake()
    {
        if (pausePanel != null)
        {
            panelRect = pausePanel.GetComponent<RectTransform>();
            pausePanel.SetActive(false);
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMenuOrQuit);
    }

    void Start()
    {
        if (panelRect == null) return;

        centerPos = Vector2.zero;
        topPos = new Vector2(0, Screen.height);
        bottomPos = new Vector2(0, -Screen.height);

        // Varmistetaan lähtötila: pois päältä ja "ylhäällä"
        panelRect.anchoredPosition = topPos;
        isPaused = false;
        isAnimating = false;

        SetPaused(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && !isAnimating)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (pausePanel == null || panelRect == null) return;
        if (isAnimating) return;

        StartCoroutine(SlideInFromTop());
    }

    public void Resume()
    {
        if (pausePanel == null || panelRect == null) return;
        if (isAnimating) return;

        StartCoroutine(SlideOutToBottomAndResetTop());
    }

    IEnumerator SlideInFromTop()
    {
        isAnimating = true;

        // AINA ylhäältä sisään
        pausePanel.SetActive(true);
        panelRect.anchoredPosition = topPos;

        // Pause päälle + hiiri käyttöön
        SetPaused(true);

        PlaySfx(wooshClip);

        while (Vector2.Distance(panelRect.anchoredPosition, centerPos) > 1f)
        {
            panelRect.anchoredPosition = Vector2.MoveTowards(
                panelRect.anchoredPosition,
                centerPos,
                slideSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        panelRect.anchoredPosition = centerPos;

        isPaused = true;
        isAnimating = false;
    }

    IEnumerator SlideOutToBottomAndResetTop()
    {
        isAnimating = true;

        // Woosh heti kun sulku alkaa
        PlaySfx(wooshClip);

        // Slide alas
        while (Vector2.Distance(panelRect.anchoredPosition, bottomPos) > 1f)
        {
            panelRect.anchoredPosition = Vector2.MoveTowards(
                panelRect.anchoredPosition,
                bottomPos,
                slideSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        panelRect.anchoredPosition = bottomPos;

        // NYT kun pullo on alhaalla -> bottle crack
        PlaySfx(bottleBreakClip);

        // Pieni hetki että ääni ehtii startata (optional)
        yield return new WaitForSecondsRealtime(0.05f);

        // Unpause ja piilota
        SetPaused(false);
        isPaused = false;

        // Reset seuraavaa avautumista varten
        panelRect.anchoredPosition = topPos;
        pausePanel.SetActive(false);

        isAnimating = false;
    }

    public void ExitToMenuOrQuit()
    {
        // Varmistetaan ettei jää pauselle
        Time.timeScale = 1f;
        isPaused = false;
        isAnimating = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (!string.IsNullOrEmpty(exitSceneName))
        {
            SceneManager.LoadScene(exitSceneName);
            return;
        }

        Application.Quit();
    }

    void OnDisable()
    {
        // jos skripti disabloituu pausessa, palautetaan aika
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    void PlaySfx(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;

        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
