using System.Collections;
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
    public InventoryService inventory;

    [Header("Scene names")]
    public string gameSceneName = "SampleScene";
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip printClip;

    [Header("Typing Settings")]
    public float typeSpeed = 0.02f;
    public float jitterAmount = 1.2f;     // kuinka paljon kirjaimet tärisee
    public float fadeSpeed = 8f;          // kuinka nopeasti alpha fade sisään

    Coroutine typingRoutine;

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

        if (inventory == null) inventory = InventoryService.Instance;
    }

    public void OpenLevelCompleteReceipt()
    {
        Open();
        FillReceipt(true);
    }

    public void OpenFailReceipt()
    {
        Open();
        FillReceipt(false);
    }

    void FillReceipt(bool success)
    {
        float seconds = (RunTimer.Instance != null)
            ? RunTimer.Instance.GetElapsedSeconds()
            : 0f;

        string timeStr = FormatTime(seconds);
        int beers = GetTotalDrinksFromInventory();
        int stars = success ? CalculateStars(seconds, beers) : 0;
        string starsStr = success
            ? new string('★', stars) + new string('☆', 5 - stars)
            : "-----";

        if (titleTMP != null)
            titleTMP.text = success ? "SUORITUS" : "EPÄONNISTUI";

        var sb = new StringBuilder();
        sb.AppendLine(success ? "SUORITUS" : "EPÄONNISTUI");
        sb.AppendLine("----------------");
        sb.AppendLine($"AIKA:   {timeStr}");
        sb.AppendLine($"JUOMAT: {beers}x");
        sb.AppendLine($"TÄHDET: {starsStr}");
        sb.AppendLine("----------------");

        StartTyping(sb.ToString());
    }

    void StartTyping(string text)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string fullText)
    {
        bodyTMP.text = "";
        bodyTMP.ForceMeshUpdate();

        if (audioSource && printClip)
        {
            audioSource.clip = printClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        foreach (char c in fullText)
        {
            bodyTMP.text += c;
            bodyTMP.ForceMeshUpdate();

            StartCoroutine(AnimateLastCharacter());

            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        if (audioSource)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    IEnumerator AnimateLastCharacter()
    {
        TMP_TextInfo textInfo = bodyTMP.textInfo;
        int charIndex = textInfo.characterCount - 1;

        if (charIndex < 0) yield break;
        if (!textInfo.characterInfo[charIndex].isVisible) yield break;

        int meshIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;

        Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;
        Color32[] colors = textInfo.meshInfo[meshIndex].colors32;

        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.unscaledDeltaTime * fadeSpeed;

            float jitterX = Random.Range(-jitterAmount, jitterAmount);
            float jitterY = Random.Range(-jitterAmount, jitterAmount);
            Vector3 offset = new Vector3(jitterX, jitterY, 0);

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;

            byte a = (byte)(Mathf.Clamp01(alpha) * 255);

            colors[vertexIndex + 0].a = a;
            colors[vertexIndex + 1].a = a;
            colors[vertexIndex + 2].a = a;
            colors[vertexIndex + 3].a = a;

            bodyTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            yield return null;
        }
    }

    int GetTotalDrinksFromInventory()
    {
        if (inventory == null) return 0;
        var items = inventory.GetReceiptItemsByName();
        if (items == null) return 0;
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
        if (!overlay) return;

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
