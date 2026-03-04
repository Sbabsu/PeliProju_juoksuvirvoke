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

    [Header("Intro settings")]
    public bool playIntroCutsceneOnlyOnce = true;
    public string introSeenKey = "INTRO_SEEN";

    [Header("Play behaviour")]
    public bool resetProgressOnPlay = true;

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

        // 1) Get up
        playerAnimator.CrossFadeInFixedTime(getUpStateName, 0.05f);
        yield return null;

        yield return WaitUntilInState(getUpStateName);

        while (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(getUpStateName) &&
               playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // 2) Walk start
        playerAnimator.SetBool(walkBoolName, true);
        playerAnimator.CrossFadeInFixedTime(walkStateName, 0.08f);

        yield return WaitUntilInState(walkStateName);
        yield return null;

        // 3) Move to target
        Vector3 targetPos = new Vector3(target.position.x, groundY, target.position.z);

        while (Vector3.Distance(new Vector3(player.position.x, groundY, player.position.z), targetPos) > stopDistance)
        {
            Vector3 cur = new Vector3(player.position.x, groundY, player.position.z);
            player.position = Vector3.MoveTowards(cur, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        playerAnimator.SetBool(walkBoolName, false);

        // 4) Tutorial
        if (goToTutorial)
        {
            SceneManager.LoadScene(tutorialSceneName);
            yield break;
        }

        // 5) Play -> cutscene only once
        bool introSeen = PlayerPrefs.GetInt(introSeenKey, 0) == 1;
        bool shouldPlayIntro = playIntroCutsceneOnlyOnce && !introSeen;

        if (shouldPlayIntro)
        {
            SceneManager.LoadScene(cutsceneSceneName);
            yield break;
        }

        // Intro already seen -> straight to first level
        GameProgress.SaveLastScene(firstLevelScene);
        SceneManager.LoadScene(firstLevelScene);
    }

    IEnumerator WaitUntilInState(string stateName)
    {
        float t = 0f;
        while (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            t += Time.deltaTime;
            if (t > 2f) break;
            yield return null;
        }
    }
}