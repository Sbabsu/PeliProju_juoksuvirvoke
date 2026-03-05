using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameSequence : MonoBehaviour
{
    [Header("Player")]
    public Animator playerAnimator;
    public Transform player;

    [Header("Targets")]
    public Transform playTarget;
    public Transform tutorialTarget;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 0.05f;

    [Header("Scenes")]
    public string tutorialSceneName = "tutorialikartta";
    public string firstLevelScene = "ekalevu";
    public string cutsceneSceneName = "MainMenu_CutScene";

    [Header("Anim State Names (exact)")]
    public string getUpStateName = "Getting Up 1";
    public string walkStateName = "walking";
    public string walkBoolName = "Walk";

    [Header("Play behaviour")]
    [Tooltip("Jos päällä, Pelaa aloittaa uuden runin (poistaa jatkon).")]
    public bool resetProgressOnPlay = true;

    [Tooltip("Näytä intro-cutscene vain kerran.")]
    public bool playIntroOnlyOnce = true;

    bool started;

    public void StartTutorial()
    {
        if (started) return;
        started = true;
        StartCoroutine(Sequence(goToTutorial: true));
    }

    public void StartPlay()
    {
        if (started) return;
        started = true;

        if (resetProgressOnPlay)
            GameProgress.ResetProgress();

        StartCoroutine(Sequence(goToTutorial: false));
    }

    IEnumerator Sequence(bool goToTutorial)
    {
        Transform target = goToTutorial ? tutorialTarget : playTarget;
        if (target == null) { started = false; yield break; }

        float groundY = player.position.y;

        // 1) Nouse ylös
        if (playerAnimator != null)
        {
            playerAnimator.CrossFadeInFixedTime(getUpStateName, 0.05f);
            yield return null;
            yield return WaitUntilInState(getUpStateName);

            while (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(getUpStateName) &&
                   playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }

            // 2) Kävely päälle
            playerAnimator.SetBool(walkBoolName, true);
            playerAnimator.CrossFadeInFixedTime(walkStateName, 0.08f);
            yield return WaitUntilInState(walkStateName);
            yield return null;
        }

        // 3) Liiku targettiin
        Vector3 targetPos = new Vector3(target.position.x, groundY, target.position.z);

        while (Vector3.Distance(new Vector3(player.position.x, groundY, player.position.z), targetPos) > stopDistance)
        {
            Vector3 cur = new Vector3(player.position.x, groundY, player.position.z);
            player.position = Vector3.MoveTowards(cur, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (playerAnimator != null)
            playerAnimator.SetBool(walkBoolName, false);

        // 4) Tutorial: suoraan tutorial-sceneen
        if (goToTutorial)
        {
            SceneManager.LoadScene(tutorialSceneName);
            yield break;
        }

        // 5) Pelaa: intro-cutscene vain ekalla kerralla
        bool introSeen = GameProgress.IsIntroSeen();
        bool shouldPlayIntro = !playIntroOnlyOnce || !introSeen;

        if (shouldPlayIntro)
        {
            // Cutscene-scene hoitaa itse siirtymisen firstLevelSceneen
            SceneManager.LoadScene(cutsceneSceneName);
            yield break;
        }

        // Jos intro jo nähty, aloita suoraan 1. levelistä
        GameProgress.SaveCurrentScene(firstLevelScene);
        SceneManager.LoadScene(firstLevelScene);
    }

    IEnumerator WaitUntilInState(string stateName)
    {
        if (playerAnimator == null) yield break;

        float t = 0f;
        while (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            t += Time.deltaTime;
            if (t > 2f) break;
            yield return null;
        }
    }
}