using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    [Header("Toggles")]
    public Toggle vsyncToggle;

    [Header("Keys")]
    public string volumeKey = "MASTER_VOLUME";
    public string sensKey = "MOUSE_SENS";
    public string vsyncKey = "VSYNC";

    void OnEnable()
    {
        float vol = PlayerPrefs.GetFloat(volumeKey, 1f);
        float sens = PlayerPrefs.GetFloat(sensKey, 1f);
        bool vsync = PlayerPrefs.GetInt(vsyncKey, 1) == 1;

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            volumeSlider.value = vol;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
            sensitivitySlider.value = sens;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (vsyncToggle != null)
        {
            vsyncToggle.onValueChanged.RemoveListener(OnVsyncChanged);
            vsyncToggle.isOn = vsync;
            vsyncToggle.onValueChanged.AddListener(OnVsyncChanged);
        }

        ApplyVolume(vol);
        ApplyVsync(vsync);
    }

    void OnDisable()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);

        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.RemoveListener(OnVsyncChanged);
    }

    void OnVolumeChanged(float v)
    {
        v = Mathf.Clamp01(v);
        ApplyVolume(v);
        PlayerPrefs.SetFloat(volumeKey, v);
        PlayerPrefs.Save();
    }

    void OnSensitivityChanged(float s)
    {
        PlayerPrefs.SetFloat(sensKey, s);
        PlayerPrefs.Save();
    }

    void OnVsyncChanged(bool enabled)
    {
        ApplyVsync(enabled);
        PlayerPrefs.SetInt(vsyncKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyVolume(float v)
    {
        AudioListener.volume = v;
    }

    void ApplyVsync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        // If VSync is off, enforce your target FPS
        if (!enabled)
            Application.targetFrameRate = 144;
    }

    public float GetSavedSensitivity() => PlayerPrefs.GetFloat(sensKey, 1f);
}