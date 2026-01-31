using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text scoreText;
    public Text pickupPromptText;
    public GameObject levelExitTrigger;
    public GameObject exitBlock; // Optional: Physical barrier that disappears

    [Header("Game Settings")]
    public int maxItems = 5;

    private int currentScore = 0;
    private bool canExitLevel = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeGame();
    }

    private void InitializeGame()
    {
        currentScore = 0;
        canExitLevel = false;
        UpdateScoreUI();

        // Initially hide level exit
        if (levelExitTrigger != null)
            levelExitTrigger.SetActive(false);

        if (exitBlock != null)
            exitBlock.SetActive(true);

        //HidePickupPrompt();
    }

    public void AddScore()
    {
        currentScore++;
        UpdateScoreUI();

        // Check if all items are collected
        if (currentScore >= maxItems)
        {
            EnableLevelExit();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Items: {currentScore}/{maxItems}";
        }
    }

    public void ShowPickupPrompt(bool show)
    {
        if (pickupPromptText != null)
        {
            pickupPromptText.gameObject.SetActive(show);
            pickupPromptText.text = "Press E to pickup";
        }
    }

    public void HidePickupPrompt()
    {
        if (pickupPromptText != null)
        {
            pickupPromptText.gameObject.SetActive(false);
        }
    }

    public void EnableLevelExit()
    {
        canExitLevel = true;

        // Activate exit trigger
        if (levelExitTrigger != null)
        {
            levelExitTrigger.SetActive(true);
        }

        // Remove any blocking obstacles
        if (exitBlock != null)
        {
            exitBlock.SetActive(false);
        }

        // Optional: Show exit prompt
        Debug.Log("All items collected! You can now exit the level.");
    }
}