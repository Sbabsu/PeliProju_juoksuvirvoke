using UnityEngine;
using System.Text;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TMP_Text listText;

    [Header("Optional: pickup feed UI")]
    public PickupFeedUI pickupFeed;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSfx;
    public AudioClip closeSfx;

    [Header("Safety")]
    [Tooltip("Estää Tabin / kutsujen tuplalaukaukset (sekunteina).")]
    public float toggleCooldown = 0.15f;

    private float nextToggleTime;
    private bool isOpen;                 // pidetään oma tila varmana
    private bool openSfxPlayedThisOpen;  // guard: open ääni vain kerran per avaus

    private InventoryService inv;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        isOpen = false;
        openSfxPlayedThisOpen = false;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.loop = false; // varmuus
    }

    void Start()
    {
        inv = InventoryService.Instance;
        Refresh();
    }

    void OnEnable()
    {
        if (inv == null) inv = InventoryService.Instance;
        if (inv != null) inv.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (inv != null) inv.OnChanged -= Refresh;
    }

    public void Toggle()
    {
        if (panel == null) return;

        // estä spam / tuplakutsu
        if (Time.unscaledTime < nextToggleTime) return;
        nextToggleTime = Time.unscaledTime + toggleCooldown;

        isOpen = !isOpen;
        panel.SetActive(isOpen);

        if (isOpen)
        {
            // Soita open vain kerran per avaus
            if (!openSfxPlayedThisOpen)
            {
                openSfxPlayedThisOpen = true;
                PlaySfx(openSfx);
            }

            Refresh();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            // Time.timeScale = 0f; // jos haluat pauselle
        }
        else
        {
            // kun suljetaan, vapautetaan open-guard seuraavaa avausta varten
            openSfxPlayedThisOpen = false;
            PlaySfx(closeSfx);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            // Time.timeScale = 1f;
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        // varmistus: jos jotain on jäänyt soimaan, pysäytä se
        // (ei pitäisi tapahtua PlayOneShotilla, mutta varmuuden vuoksi)
        audioSource.Stop();
        audioSource.loop = false;

        audioSource.PlayOneShot(clip);
    }

    public void Refresh()
    {
        if (listText == null) return;
        if (inv == null)
        {
            listText.text = "InventoryService missing";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("INVENTORY");

        var all = inv.GetAll();
        if (all.Count == 0)
        {
            sb.AppendLine("(nothing yet)");
        }
        else
        {
            foreach (var kvp in all)
            {
                var name = inv.GetDisplayName(kvp.Key);
                sb.AppendLine($"{name} x{kvp.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("TAB = open/close");
        listText.text = sb.ToString();
    }

    public void ShowPickup(string itemId)
    {
        if (pickupFeed == null || inv == null) return;

        string name = inv.GetDisplayName(itemId);
        Sprite icon = inv.GetIcon(itemId);
        int count = inv.GetCount(itemId);

        pickupFeed.Show(name, count, icon);
    }
}
