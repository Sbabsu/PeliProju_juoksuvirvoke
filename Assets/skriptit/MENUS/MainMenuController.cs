using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings Camera")]
    public SettingsCameraRig settingsRig;   // vedä SettingsCameraRig GO tähän
    public Button settingsCloseButton;      // asetusten "Takaisin" nappi

    void Start()
    {
        // Jatka näkyy vasta kun progress sallii
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(GameProgress.IsContinueUnlocked());
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueGame);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() =>
            {
                if (settingsRig != null) settingsRig.OpenSettings();
            });
        }

        if (settingsCloseButton != null)
        {
            settingsCloseButton.onClick.RemoveAllListeners();
            settingsCloseButton.onClick.AddListener(() =>
            {
                if (settingsRig != null) settingsRig.CloseSettings();
            });
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(Application.Quit);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;
    }

    public void ContinueGame()
    {
        string scene = GameProgress.GetNextLevelScene();
        if (!string.IsNullOrEmpty(scene))
            SceneManager.LoadScene(scene);
    }
}