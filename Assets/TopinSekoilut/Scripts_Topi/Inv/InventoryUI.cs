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
    public float toggleCooldown = 0.15f;

    private float nextToggleTime;
    private bool isOpen;
    private bool openSfxPlayedThisOpen;

    private InventoryService inv;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        isOpen = false;
        openSfxPlayedThisOpen = false;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    void Start()
    {
        inv = InventoryService.Instance;

        if (inv == null)
        {
            Debug.LogWarning("InventoryUI: InventoryService.Instance is still null in Start");
            return;
        }

        inv.OnChanged += Refresh;
        inv.OnItemAdded += HandleItemAdded;

        Refresh();
    }

    void OnDestroy()
    {
        if (inv != null)
        {
            inv.OnChanged -= Refresh;
            inv.OnItemAdded -= HandleItemAdded;
        }
    }

    public void Toggle()
    {
        if (panel == null) return;
        if (PauseMenu.IsPauseBlockingInput) return;
        if (Time.unscaledTime < nextToggleTime) return;

        nextToggleTime = Time.unscaledTime + toggleCooldown;

        isOpen = !isOpen;
        panel.SetActive(isOpen);

        if (isOpen)
        {
            if (!openSfxPlayedThisOpen)
            {
                openSfxPlayedThisOpen = true;
                PlayUiSfx(openSfx);
            }

            Refresh();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
        }
        else
        {
            openSfxPlayedThisOpen = false;
            PlayUiSfx(closeSfx);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void PlayUiSfx(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

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

    private void HandleItemAdded(string itemId, int amount)
    {
        ShowPickup(itemId);

        if (inv == null) return;

        var def = inv.GetDefinition(itemId);
        if (def == null) return;

        if (def.pickupSoundClip != null && def.pickupSoundClip.Length > 0)
        {
            int randomIndex = Random.Range(0, def.pickupSoundClip.Length);
            AudioClip clip = def.pickupSoundClip[randomIndex];
            PlayPickupSfx(clip, def.pickupVolume);
        }
    }

    private void PlayPickupSfx(AudioClip clip, float volume = 1f)
    {
        if (audioSource == null || clip == null) return;

        audioSource.PlayOneShot(clip, volume);
    }
}