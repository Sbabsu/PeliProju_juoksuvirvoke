using UnityEngine;

public class SpeedBoostPowerUp : MonoBehaviour
{
    public float multiplier = 1.2f; // +20%
    public float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        AbilityUIManager.Instance.Show("Speed Boost +20%!", 2f);
        player.ApplySpeedMultiplier(multiplier, duration);

        Destroy(gameObject);
    }
}
