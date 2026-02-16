using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GuardSlowPowerUp : MonoBehaviour
{
    public float slowMultiplier = 0.5f;
    public float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        AbilityUIManager.Instance.Show("Guards Slowed 50%!", 2f);
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
}