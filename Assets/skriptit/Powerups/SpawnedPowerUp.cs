using UnityEngine;

public class SpawnedPowerUp : MonoBehaviour
{
    [HideInInspector] public PowerUpSpawner spawner;

    private void OnDestroy()
    {
        if (spawner != null) spawner.OnPickupGone();
    }
}
