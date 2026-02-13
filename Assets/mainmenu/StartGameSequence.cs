using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameSequence : MonoBehaviour
{
    public Animator playerAnimator;
    public Transform player;
    public Transform target;

    public float moveSpeed = 2f;
    public float stopDistance = 0.05f;

    public string sceneName = "GameScene";

    bool started;

    public void StartGame()
    {
        if (started) return;
        started = true;

        StartCoroutine(WalkToTarget());
    }

    IEnumerator WalkToTarget()
    {
        // Käynnistä kävelyanimaatio
        playerAnimator.SetBool("Walk", true);

        // Liiku kohti targettia
        while (Vector3.Distance(player.position, target.position) > stopDistance)
        {
            player.position = Vector3.MoveTowards(
                player.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Pysäytä animaatio
        playerAnimator.SetBool("Walk", false);

        // Pieni viive
        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene(sceneName);
    }
}
