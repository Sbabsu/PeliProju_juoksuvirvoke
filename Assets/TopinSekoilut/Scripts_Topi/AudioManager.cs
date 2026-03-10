using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("InventoryAudio: Duplicate instance found, destroying the new one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            Debug.LogWarning("InventoryAudio: No AudioSource assigned or found on this object.");
        else
            Debug.Log("InventoryAudio: Using AudioSource on " + sfxSource.gameObject.name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayPickup(string itemId)
    {
        Debug.Log("InventoryAudio.PlayPickup called with itemId: " + itemId);

        if (sfxSource == null)
        {
            Debug.LogWarning("InventoryAudio: sfxSource is null.");
            return;
        }

        if (InventoryService.Instance == null)
        {
            Debug.LogWarning("InventoryAudio: InventoryService.Instance is null.");
            return;
        }

        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("InventoryAudio: itemId is null or empty.");
            return;
        }

 

        if (clip == null)
        {
            Debug.LogWarning("InventoryAudio: No pickup clip found for itemId: " + itemId);
            return;
        }

        Debug.Log("InventoryAudio: Playing clip " + clip.name);
        sfxSource.PlayOneShot(clip);
    }
}
