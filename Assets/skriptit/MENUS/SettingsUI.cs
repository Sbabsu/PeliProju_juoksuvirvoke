using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    [Header("Keys")]
    public string volumeKey = "MASTER_VOLUME";
    public string sensKey = "MOUSE_SENS";

    void OnEnable()
    {
        float vol = PlayerPrefs.GetFloat(volumeKey, 1f);
        float sens = PlayerPrefs.GetFloat(sensKey, 1f);

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

        ApplyVolume(vol);
    }

    void OnDisable()
    {
        if (volumeSlider != null) volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        if (sensitivitySlider != null) sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
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
        // anna sun sliderille vaikka 0.2–3.0 range Inspectorissa
        PlayerPrefs.SetFloat(sensKey, s);
        PlayerPrefs.Save();
    }

    void ApplyVolume(float v)
    {
        AudioListener.volume = v;
    }

    public float GetSavedSensitivity() => PlayerPrefs.GetFloat(sensKey, 1f);
}