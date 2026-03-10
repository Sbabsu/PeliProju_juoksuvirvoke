using System.Collections;
using UnityEngine;

public class GuardSlowPowerUp : MonoBehaviour
{
    public float slowMultiplier = 0.5f;
    public float duration = 5f;

    [Header("Audio")]
    [SerializeField] AudioClip[] pickupSoundClip;
    [SerializeField] float volume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        AbilityUIManager.Instance.Show("Guards Slowed 50%!", 2f);

        PlayPickupSound();

        StartCoroutine(SlowRoutine());

        Destroy(gameObject);
    }

    IEnumerator SlowRoutine()
    {
        var guards = FindObjectsByType<GuardStateMachine>(FindObjectsSortMode.None);

        foreach (var g in guards)
        {
            g.ApplySpeedMultiplier(slowMultiplier, duration);
        }

        yield return null;
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