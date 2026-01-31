using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text scoreText;

    [Header("Level Complete Popup")]
    public GameObject levelCompletePopup;
    public TMP_Text popupScoreText;
    public Button menuButton;

    [Header("Game Settings")]
    public int maxItems = 5;

    private int currentScore = 0;
    private float levelStartTime;
    private bool isLevelComplete = false;
    private bool exitEnabled = false; // Track if exit is actually enabled

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        levelStartTime = Time.time;
        UpdateScoreUI();

        if (levelCompletePopup != null)
            levelCompletePopup.SetActive(false);

        exitEnabled = false;
    }

    public void AddScore()
    {
        if (isLevelComplete) return;

        currentScore++;
        UpdateScoreUI();

        // Only enable exit when we have ALL required items
        if (currentScore >= maxItems && !exitEnabled)
        {
            EnableExit();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Items: {currentScore}/{maxItems}";
        }
    }

    private void EnableExit()
    {
        exitEnabled = true;
        Debug.Log("All items collected! Exit is now enabled.");
    }

    public void ShowLevelCompletePopup()
    {
        // Only show popup if player actually completed the level
        if (isLevelComplete || !exitEnabled) return;

        isLevelComplete = true;

        // Pause the game
        Time.timeScale = 0f;

        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Setup popup
        if (levelCompletePopup != null)
        {
            levelCompletePopup.SetActive(true);

            // Calculate level time
            float levelTime = Time.time - levelStartTime;
            int minutes = Mathf.FloorToInt(levelTime / 60f);
            int seconds = Mathf.FloorToInt(levelTime % 60f);

            // Update popup text
            if (popupScoreText != null)
                popupScoreText.text = $"Items Collected: {currentScore}/{maxItems}";
        }

        // Setup button events

        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);
    }

    public void ReturnToMenu()
    {
        // Resume time before loading menu
        Time.timeScale = 1f;

        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Add this method to check if exit is enabled
    public bool IsExitEnabled()
    {
        return exitEnabled;
    }
}