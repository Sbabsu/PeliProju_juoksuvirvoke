using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button exitButton;

    [Header("Exit target")]
    public string exitSceneName = "MainMenu"; // tai tyhjä jos haluat Application.Quit

    [Header("Options")]
    public KeyCode toggleKey = KeyCode.Escape;

    bool isPaused;

    void Awake()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMenuOrQuit);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (pausePanel == null) return;

        isPaused = true;
        pausePanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (pausePanel == null) return;

        isPaused = false;
        pausePanel.SetActive(false);

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ExitToMenuOrQuit()
    {
        Time.timeScale = 1f;

        // Jos haluat aina main menuun:
        if (!string.IsNullOrEmpty(exitSceneName))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene(exitSceneName);
            return;
        }

        // Muuten quit
        Application.Quit();
    }

    // Jos vaihdat sceneä pause päällä, varmistetaan ettei jäädy
    void OnDisable()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
    }
}
