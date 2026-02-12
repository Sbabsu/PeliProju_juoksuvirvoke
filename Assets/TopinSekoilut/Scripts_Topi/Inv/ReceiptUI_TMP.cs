using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReceiptUI_TMP : MonoBehaviour
{
    [Header("UI Refs")]
    public GameObject overlay;
    public TMP_Text titleTMP;
    public TMP_Text bodyTMP;
    public Button retryButton;
    public Button mainMenuButton;

    [Header("Data")]
    public InventoryService inventory; // ✅ NEW

    [Header("Scene names")]
    public string gameSceneName = "SampleScene";
    public string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        if (overlay != null) overlay.SetActive(false);

        if (retryButton != null) retryButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        });

        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        });

        if (bodyTMP != null)
        {
            bodyTMP.enableWordWrapping = false;
            bodyTMP.overflowMode = TextOverflowModes.Overflow;
        }

        if (inventory == null) inventory = InventoryService.Instance;
    }

    public void OpenLevelCompleteReceipt()
    {
        Open();

        if (inventory == null) inventory = InventoryService.Instance;

        float start = (GameManager.Instance != null) ? GameManager.Instance.RunStartRealtime : 0f;
        float seconds = Time.realtimeSinceStartup - start;
        string timeStr = FormatTime(seconds);

        int beers = GetTotalDrinksFromInventory();
        int stars = CalculateStars(seconds, beers);
        string starsStr = new string('★', stars) + new string('☆', 5 - stars);

        var sb = new StringBuilder();
        sb.AppendLine("SUORITUS");
        sb.AppendLine("----------------");
        sb.AppendLine($"AIKA:   {timeStr}");
        sb.AppendLine($"JUOMAT: {beers}x");
        sb.AppendLine($"TÄHDET: {starsStr}");
        sb.AppendLine("----------------");

        if (bodyTMP != null) bodyTMP.text = sb.ToString();
    }

    int GetTotalDrinksFromInventory()
    {
        if (inventory == null) return 0;

        // Uses the new service method (displayName -> count)
        var items = inventory.GetReceiptItemsByName();
        if (items == null || items.Count == 0) return 0;

        return items.Sum(k => Mathf.Max(0, k.Value));
    }

    int CalculateStars(float seconds, int beers)
    {
        if (beers <= 0) return 1;

        if (beers >= 5 && seconds <= 20f) return 5;
        if ((beers >= 4 && seconds <= 20f) || (beers >= 5 && seconds <= 30f)) return 4;
        if ((beers >= 3 && seconds <= 25f) || (beers >= 4 && seconds <= 30f)) return 3;
        if ((beers >= 2 && seconds <= 30f) || (beers >= 3 && seconds <= 45f)) return 2;

        return 1;
    }

    void Open()
    {
        if (overlay == null) return;

        overlay.SetActive(true);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        EnsureEventSystem();
    }

    static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        return $"{min:00}:{sec:00}";
    }
}
