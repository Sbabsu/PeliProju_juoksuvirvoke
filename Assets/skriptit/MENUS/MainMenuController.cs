using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Sequence")]
    public StartGameSequence startGameSequence;

    [Header("Buttons")]
    public Button tutorialButton;
    public Button playButton;
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings UI (vain panelin aktivointi)")]
    public GameObject settingsPanelRoot;
    public Button settingsCloseButton;

    void Start()
    {
        // Nappien kuuntelijat
        if (tutorialButton) tutorialButton.onClick.AddListener(OnTutorial);
        if (playButton) playButton.onClick.AddListener(OnPlay);
        if (continueButton) continueButton.onClick.AddListener(OnContinue);
        if (settingsButton) settingsButton.onClick.AddListener(OnSettings);
        if (settingsCloseButton) settingsCloseButton.onClick.AddListener(OnSettingsClose);
        if (quitButton) quitButton.onClick.AddListener(OnQuit);

        RefreshContinueVisibility();
        if (settingsPanelRoot) settingsPanelRoot.SetActive(false);
    }

    void RefreshContinueVisibility()
    {
        if (continueButton)
            continueButton.gameObject.SetActive(GameProgress.IsContinueUnlocked());
    }

    void OnTutorial()
    {
        if (startGameSequence != null) startGameSequence.StartTutorial();
    }

    void OnPlay()
    {
        if (startGameSequence != null) startGameSequence.StartPlay();
    }

    void OnContinue()
    {
        // “Jatka” vain jos avattu
        if (!GameProgress.IsContinueUnlocked())
        {
            RefreshContinueVisibility();
            return;
        }

        string scene = GameProgress.GetLastScene();
        if (string.IsNullOrEmpty(scene))
        {
            RefreshContinueVisibility();
            return;
        }

        SceneManager.LoadScene(scene);
    }

    void OnSettings()
    {
        if (settingsPanelRoot) settingsPanelRoot.SetActive(true);
        if (continueButton) continueButton.interactable = false;
        if (playButton) playButton.interactable = false;
        if (tutorialButton) tutorialButton.interactable = false;
        if (quitButton) quitButton.interactable = false;
        if (settingsButton) settingsButton.interactable = false;
    }

    void OnSettingsClose()
    {
        if (settingsPanelRoot) settingsPanelRoot.SetActive(false);

        if (continueButton) continueButton.interactable = true;
        if (playButton) playButton.interactable = true;
        if (tutorialButton) tutorialButton.interactable = true;
        if (quitButton) quitButton.interactable = true;
        if (settingsButton) settingsButton.interactable = true;

        RefreshContinueVisibility();
    }

    void OnQuit()
    {
        Application.Quit();
    }
}