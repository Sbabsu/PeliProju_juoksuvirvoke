using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene names (must be in Build Settings)")]
    public string gameSceneName = "kartta1";

    void OnEnable()
    {
        // Varmista että mainmenussa hiiri näkyy ja aika kulkee
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartGame()
    {
        Debug.Log("STARTGAME CLICKED");

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT CLICKED");
        Application.Quit();
    }
}


