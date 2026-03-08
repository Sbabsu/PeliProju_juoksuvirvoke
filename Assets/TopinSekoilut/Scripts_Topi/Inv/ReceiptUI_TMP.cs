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

    [Header("Jatka (vain onnistumisessa)")]
    public Button nextLevelButton;     // liitä inspectorissa
    private string nextLevelSceneName = "";

    [Header("Caps UI (tippuvat korkit)")]
    public CapRatingUI capRatingUI;    // liitä CapsRow (jossa CapRatingUI)
    private int pendingCaps = 0;
    private bool capsAlreadyTriggered = false;

    [Header("Data")]
    public InventoryService inventory;

    [Header("Scene names")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip printClip;

    [Header("Typing Settings")]
    public float typeSpeed = 0.02f;
    public float jitterAmount = 1.2f;
    public float fadeSpeed = 8f;

    private Coroutine typingRoutine;

    // Piilomerkki tekstiin: kun typewriter saavuttaa tämän, trigataan korkkirivi
    private const string CAPS_MARKER = "<CAPS_ROW>";

    void Awake()
    {
        if (overlay != null) overlay.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryCurrentScene);

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(mainMenuSceneName);
            });
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;

                if (!string.IsNullOrEmpty(nextLevelSceneName))
                    SceneManager.LoadScene(nextLevelSceneName);
                else
                    SceneManager.LoadScene(mainMenuSceneName);
            });

            nextLevelButton.gameObject.SetActive(false);
        }

        if (inventory == null) inventory = InventoryService.Instance;

        if (capRatingUI != null)
            capRatingUI.HideRow();
    }

    private void RetryCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // -------------------------
    // PUBLIC API
    // -------------------------

    // Kutsu tätä kun level läpäistään
    public void OpenLevelCompleteReceipt()
    {
        RunResult.CaptureFromGameManager(); // <-- capture time first

        string current = SceneManager.GetActiveScene().name;
        nextLevelSceneName = GameProgress.OnLevelCompleted(current);

        if (nextLevelButton != null)
        {
            bool hasNext = !string.IsNullOrEmpty(nextLevelSceneName);
            nextLevelButton.gameObject.SetActive(hasNext);
            nextLevelButton.interactable = hasNext;
        }

        Open();
        FillReceipt(success: true);
    }

    // Kutsu tätä kun jää kiinni vartijalle / fail
    public void OpenFailReceipt()
    {
        RunResult.CaptureFromGameManager(); // <-- capture time first

        nextLevelSceneName = "";

        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
            nextLevelButton.interactable = false;
        }

        Open();
        FillReceipt(success: false);
    }

    // -------------------------
    // RECEIPT CONTENT
    // -------------------------

    void FillReceipt(bool success)
    {
        float seconds = RunResult.lastRunSeconds; // STORED VALUE

        string timeStr = FormatTime(seconds);
        int beers = GetTotalDrinksFromInventory();

        bool caughtByGuard = !success;

        // 0–5 korkkia
        pendingCaps = CalculateCaps(beers, caughtByGuard);
        capsAlreadyTriggered = false;

        if (titleTMP != null)
            titleTMP.text = success ? "SUORITUS" : "EPÄONNISTUI";

        // Valmistele caps UI (piilossa) – asetetaan tiputettavien määrä valmiiksi
        if (capRatingUI != null)
        {
            capRatingUI.ShowRow();       // voidaan pitää aktiivisena layoutin takia
            capRatingUI.SetDropCount(0); // piilota kaikki ensin
            capRatingUI.HideRow();       // mutta koko rivi piiloon kunnes marker saavutetaan
        }

        var sb = new StringBuilder();
        sb.AppendLine(success ? "SUORITUS" : "EPÄONNISTUI");
        sb.AppendLine("----------------");
        sb.AppendLine($"AIKA:   {timeStr}");
        sb.AppendLine($"JUOMAT: {beers}x");
        sb.AppendLine("KORKIT:");
        sb.AppendLine(CAPS_MARKER); // <-- triggaa caps-rivin näkyviin + drop
        sb.AppendLine("----------------");

        StartTyping(sb.ToString());
    }

    int GetTotalDrinksFromInventory()
    {
        if (inventory == null) return 0;
        var items = inventory.GetReceiptItemsByName();
        if (items == null) return 0;
        return items.Sum(k => Mathf.Max(0, k.Value));
    }

    // -------------------------
    // GRADING (caps)
    // -------------------------

    int CalculateCaps(int beers, bool caughtByGuard)
    {
        if (caughtByGuard) return 0;

        // 0 = alle 4
        // 1 = 4+
        // 2 = 6+
        // 3 = 8+
        // 4 = 10
        // 5 = >10
        if (beers >= 11) return 5;
        if (beers >= 10) return 4;
        if (beers >= 8)  return 3;
        if (beers >= 6)  return 2;
        if (beers >= 4)  return 1;
        return 0;
    }

    // -------------------------
    // OPEN / INPUT
    // -------------------------

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

    // -------------------------
    // TYPEWRITER + CAPS SYNC
    // -------------------------

    void StartTyping(string text)
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string fullText)
    {
        if (bodyTMP == null) yield break;

        bodyTMP.text = "";
        bodyTMP.ForceMeshUpdate();

        if (audioSource && printClip)
        {
            audioSource.clip = printClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        for (int i = 0; i < fullText.Length; i++)
        {
            // Kun marker saavutetaan → näytä caps row + tiputa vain pendingCaps määrä
            if (!capsAlreadyTriggered && StartsWithAt(fullText, CAPS_MARKER, i))
            {
                capsAlreadyTriggered = true;

                if (capRatingUI != null)
                {
                    capRatingUI.ShowRow();
                    capRatingUI.SetDropCount(pendingCaps);

                    if (pendingCaps > 0)
                        capRatingUI.PlayDrop(pendingCaps);
                }

                // Ohita marker (ei näy tekstissä)
                i += CAPS_MARKER.Length - 1;

                // pieni “rivivaihto” fiilis
                yield return new WaitForSecondsRealtime(typeSpeed);
                continue;
            }

            // normaali kirjain printti
            char c = fullText[i];
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

    static bool StartsWithAt(string s, string needle, int index)
    {
        if (index + needle.Length > s.Length) return false;
        for (int j = 0; j < needle.Length; j++)
            if (s[index + j] != needle[j]) return false;
        return true;
    }

    IEnumerator AnimateLastCharacter()
    {
        if (bodyTMP == null) yield break;

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

    string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        return $"{min:00}:{sec:00}";
    }

    private void CaptureFinalTime()
    {
        RunResult.lastRunSeconds = (RunTimer.Instance != null)
            ? RunTimer.Instance.GetElapsedSeconds()
            : 0f;
    }
}