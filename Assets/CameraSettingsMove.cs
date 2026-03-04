using System.Collections;
using UnityEngine;

public class CameraSettingsMove : MonoBehaviour
{
    [Header("Points")]
    public Transform menuPoint;
    public Transform settingsPoint;

    [Header("UI")]
    public GameObject settingsPanel; // sun asetukset-paneeli (sliderit), optional

    [Header("Move")]
    public float moveDuration = 0.35f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Behaviour")]
    public bool pauseTimeInSettings = true;

    Coroutine routine;
    bool inSettings;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // varmistus: jos menuPoint on asetettu, snapataan alussa siihen
        if (menuPoint != null)
        {
            transform.position = menuPoint.position;
            transform.rotation = menuPoint.rotation;
        }
    }

    public void OpenSettings()
    {
        if (inSettings) return;
        inSettings = true;

        if (pauseTimeInSettings) Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        MoveTo(settingsPoint);
    }

    public void CloseSettings()
    {
        if (!inSettings) return;
        inSettings = false;

        if (pauseTimeInSettings) Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        MoveTo(menuPoint, after: () =>
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        });
    }

    public void ToggleSettings()
    {
        if (inSettings) CloseSettings();
        else OpenSettings();
    }

    void MoveTo(Transform target, System.Action after = null)
    {
        if (target == null)
        {
            Debug.LogError("[CameraSettingsMove] Target point is missing!");
            return;
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(MoveRoutine(target.position, target.rotation, after));
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot, System.Action after)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        float dur = Mathf.Max(0.01f, moveDuration);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            float k = ease.Evaluate(Mathf.Clamp01(t));

            transform.position = Vector3.Lerp(startPos, targetPos, k);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, k);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        after?.Invoke();
        routine = null;
    }
}