using System.Collections;
using UnityEngine;

public class SettingsCameraRig : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraTransform;   // vedä tähän main menu camera (tai Cinemachine Virtual Camera parent)
    
    [Header("Targets")]
    public Transform menuView;          // missä kamera on normaalisti (tyhjä transform)
    public Transform settingsView;      // missä kamera on asetuksissa (tyhjä transform)

    [Header("UI")]
    public GameObject settingsPanel;    // asetuspaneeli (sliderit) - worldspace tai screenspace

    [Header("Move")]
    public float moveDuration = 0.35f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Pause / Cursor")]
    public bool pauseTimeWhileInSettings = true;

    Coroutine routine;
    bool inSettings;

    void Awake()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ToggleSettings()
    {
        if (inSettings) CloseSettings();
        else OpenSettings();
    }

    public void OpenSettings()
    {
        if (inSettings) return;
        inSettings = true;

        if (pauseTimeWhileInSettings) Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        StartMove(menuView, settingsView);
    }

    public void CloseSettings()
    {
        if (!inSettings) return;
        inSettings = false;

        if (pauseTimeWhileInSettings) Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        StartMove(settingsView, menuView, after: () =>
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        });
    }

    void StartMove(Transform from, Transform to, System.Action after = null)
    {
        if (cameraTransform == null || from == null || to == null)
        {
            Debug.LogError("[SettingsCameraRig] Missing references (cameraTransform/menuView/settingsView).");
            return;
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(MoveRoutine(from, to, after));
    }

    IEnumerator MoveRoutine(Transform from, Transform to, System.Action after)
    {
        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;

        Vector3 endPos = to.position;
        Quaternion endRot = to.rotation;

        // Jos kamera ei ole exact "from"-paikassa, ei haittaa: lähdetään currentista.
        float t = 0f;
        float dur = Mathf.Max(0.01f, moveDuration);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            float k = ease.Evaluate(Mathf.Clamp01(t));

            cameraTransform.position = Vector3.Lerp(startPos, endPos, k);
            cameraTransform.rotation = Quaternion.Slerp(startRot, endRot, k);

            yield return null;
        }

        cameraTransform.position = endPos;
        cameraTransform.rotation = endRot;

        after?.Invoke();
        routine = null;
    }
}