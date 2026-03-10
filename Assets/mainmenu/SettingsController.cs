using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsController : MonoBehaviour
{
    [Header("UI")]
    public Slider mouseSlider;
    public Slider volumeSlider;
    public Toggle vsyncToggle;

    public TMP_Text mouseValueText;
    public TMP_Text volumeValueText;

    const string MOUSE_KEY = "mouse_sensitivity";
    const string VOLUME_KEY = "volume";
    const string VSYNC_KEY = "vsync";

    void Start()
    {
        float mouse = PlayerPrefs.GetFloat(MOUSE_KEY, 1f);
        float volume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        bool vsync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1;

        if (mouseSlider != null)
        {
            mouseSlider.minValue = 0.2f;
            mouseSlider.maxValue = 3f;
            mouseSlider.value = mouse;
            mouseSlider.onValueChanged.AddListener(OnMouseChanged);
        }

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = volume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = vsync;
            vsyncToggle.onValueChanged.AddListener(OnVsyncChanged);
        }

        ApplyMouse(mouse);
        ApplyVolume(volume);
        ApplyVsync(vsync);

        UpdateTexts(mouse, volume);
    }

    void OnDestroy()
    {
        if (mouseSlider != null)
            mouseSlider.onValueChanged.RemoveListener(OnMouseChanged);

        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);

        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.RemoveListener(OnVsyncChanged);
    }

    void OnMouseChanged(float value)
    {
        ApplyMouse(value);
        PlayerPrefs.SetFloat(MOUSE_KEY, value);
        PlayerPrefs.Save();

        UpdateTexts(value, volumeSlider != null ? volumeSlider.value : 1f);
    }

    void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        PlayerPrefs.Save();

        UpdateTexts(mouseSlider != null ? mouseSlider.value : 1f, value);
    }

    void OnVsyncChanged(bool enabled)
    {
        ApplyVsync(enabled);
        PlayerPrefs.SetInt(VSYNC_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyMouse(float value)
    {
        // myöhemmin voidaan liittää kameraan
    }

    void ApplyVolume(float value)
    {
        AudioListener.volume = value;
    }

    void ApplyVsync(bool enabled)
    {
        if (enabled)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 144;
        }
    }

    void UpdateTexts(float mouse, float volume)
    {
        if (mouseValueText != null)
            mouseValueText.text = mouse.ToString("0.00");

        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(volume * 100) + "%";
    }
}