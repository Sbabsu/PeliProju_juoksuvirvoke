using UnityEngine;

public class SpeedBoostPowerUp : MonoBehaviour
{
    public float multiplier = 1.5f;
    public float duration = 5f;

    [Header("Audio")]
    [SerializeField] AudioClip[] pickupSoundClip;
    [SerializeField] float volume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        player.ApplySpeedMultiplier(multiplier, duration);

        if (AbilityUIManager.Instance != null)
            AbilityUIManager.Instance.Show("Speed Boost!", 2f);

        PlayPickupSound();

        Destroy(gameObject);
    }

    void PlayPickupSound()
    {
        if (pickupSoundClip == null || pickupSoundClip.Length == 0) return;

        int randomIndex = Random.Range(0, pickupSoundClip.Length);
        AudioClip clip = pickupSoundClip[randomIndex];

        AudioSource source = new GameObject("tempAudio").AddComponent<AudioSource>();
        source.transform.position = Camera.main.transform.position;
        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.Play();
        Destroy(source.gameObject, clip.length);
    }
}