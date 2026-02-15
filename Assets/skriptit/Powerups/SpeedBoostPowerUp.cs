using UnityEngine;

public class SpeedBoostPowerUp : MonoBehaviour
{
    public float multiplier = 1.5f;   // make it noticeable
    public float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        player.ApplySpeedMultiplier(multiplier, duration);

        if (AbilityUIManager.Instance != null)
            AbilityUIManager.Instance.Show("Speed Boost!", 2f);

        Destroy(gameObject);
    }
}
