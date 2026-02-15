using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
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
    public string playSceneName = "MainMenu_CutScene";
    public string tutorialSceneName = "kartta1";

    [Header("Anim State Names (exact)")]
    public string getUpStateName = "Getting Up 1";
    public string walkStateName = "walking"; // sun Animator-staten nimi
    public string walkBoolName = "Walk";

    [Header("Cutscene (Play only)")]
    public PlayableDirector cutsceneDirector;
    public GameObject uiRoot;

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
        StartCoroutine(Sequence(goToTutorial: false));
    }

    IEnumerator Sequence(bool goToTutorial)
    {
        Transform target = goToTutorial ? tutorialTarget : playTarget;
        if (target == null) { started = false; yield break; }

        // Lukitaan "groundY" heti alussa ettei y heilu
        float groundY = player.position.y;

        // 1) Nouse ylös
        playerAnimator.CrossFadeInFixedTime(getUpStateName, 0.05f);
        yield return null;

        // Odota että getup on oikeasti käynnissä
        yield return WaitUntilInState(getUpStateName);

        // Odota että getup loppuu kunnolla
        while (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(getUpStateName) &&
               playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // 2) Laita kävely päälle, MUTTA älä liikuta vielä
        playerAnimator.SetBool(walkBoolName, true);
        playerAnimator.CrossFadeInFixedTime(walkStateName, 0.08f);

        // TÄRKEIN: odota että animator on varmasti walking-statessa ennen liikkumista
        yield return WaitUntilInState(walkStateName);

        // pieni “buffer” ettei eka frame liiku ennen posea
        yield return null;

        // 3) Liiku targettiin (Y lukittu)
        Vector3 targetPos = new Vector3(target.position.x, groundY, target.position.z);

        while (Vector3.Distance(new Vector3(player.position.x, groundY, player.position.z), targetPos) > stopDistance)
        {
            Vector3 cur = new Vector3(player.position.x, groundY, player.position.z);
            player.position = Vector3.MoveTowards(cur, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Pysäytä kävely
        playerAnimator.SetBool(walkBoolName, false);

        // 4) Tutoriaali: suoraan scene
        if (goToTutorial)
        {
            SceneManager.LoadScene(tutorialSceneName);
            yield break;
        }

        // 5) Pelaa: cutscene
        if (uiRoot) uiRoot.SetActive(false);
        if (player) player.gameObject.SetActive(false);

        // jos haluat: odota 0.1s ettei tule “pysähdysframe”
        yield return null;

        if (cutsceneDirector != null)
        {
            cutsceneDirector.time = 0;
            cutsceneDirector.Play();
            while (cutsceneDirector.state == PlayState.Playing) yield return null;
        }

        SceneManager.LoadScene(playSceneName);
    }

    IEnumerator WaitUntilInState(string stateName)
    {
        // odota max ~2s ettei jää ikuisesti jumiin jos nimi väärin
        float t = 0f;
        while (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            t += Time.deltaTime;
            if (t > 2f) break;
            yield return null;
        }
    }
}

