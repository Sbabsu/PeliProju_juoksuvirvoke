using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("References")]
    public Transform character;          // vedä hahmo tähän Inspectorissa
    public Animator animator;            // vedä hahmon Animator tähän
    public string walkBoolName = "Walk"; // Animatorin bool-parametri (tee tämä Animatoriin)
    
    [Header("Exit walk")]
    public Transform exitTarget;          // tyhjä GameObject ruudun ulkopuolelle (minne kävellään)
    public float walkSpeed = 2.0f;

    [Header("Scene")]
    public string sceneToLoad = "GameScene";

    bool started;

    public void PlayGame()
    {
        if (started) return;
        started = true;
        StartCoroutine(ExitAndLoad());
    }

    IEnumerator ExitAndLoad()
    {
        // kävelyanimaatio päälle
        if (animator != null)
            animator.SetBool(walkBoolName, true);

        // kävely kohti kohdetta
        while (Vector3.Distance(character.position, exitTarget.position) > 0.05f)
        {
            character.position = Vector3.MoveTowards(
                character.position,
                exitTarget.position,
                walkSpeed * Time.deltaTime
            );

            // käännä hahmo kohti suuntaa (valinnainen)
            Vector3 dir = (exitTarget.position - character.position);
            dir.y = 0;
            if (dir.sqrMagnitude > 0.0001f)
                character.forward = dir.normalized;

            yield return null;
        }

        // animaatio pois (valinnainen)
        if (animator != null)
            animator.SetBool(walkBoolName, false);

        // lataa seuraava kartta/scenet
        SceneManager.LoadScene(sceneToLoad);
    }
}
