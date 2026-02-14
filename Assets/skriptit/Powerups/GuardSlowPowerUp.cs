using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardSlowPowerUp : MonoBehaviour
{
    public float slowMultiplier = 0.5f; // -50%
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
        GameObject[] guards = GameObject.FindGameObjectsWithTag("Guard");
        Dictionary<NavMeshAgent, float> original = new Dictionary<NavMeshAgent, float>();

        foreach (var g in guards)
        {
            var agent = g.GetComponent<NavMeshAgent>();
            if (agent == null) continue;

            original[agent] = agent.speed;
            agent.speed = agent.speed * slowMultiplier;
        }

        yield return new WaitForSeconds(duration);

        foreach (var kv in original)
        {
            if (kv.Key != null) kv.Key.speed = kv.Value;
        }
    }
}
