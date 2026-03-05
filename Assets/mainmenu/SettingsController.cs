using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsController : MonoBehaviour
{
    [Header("UI")]
    public Slider mouseSlider;
    public Slider volumeSlider;

    public TMP_Text mouseValueText;
    public TMP_Text volumeValueText;

    const string MOUSE_KEY = "mouse_sensitivity";
    const string VOLUME_KEY = "volume";

    void Start()
    {
        float mouse = PlayerPrefs.GetFloat(MOUSE_KEY, 1f);
        float volume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);

        mouseSlider.minValue = 0.2f;
        mouseSlider.maxValue = 3f;
        mouseSlider.value = mouse;

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = volume;

        mouseSlider.onValueChanged.AddListener(OnMouseChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        ApplyMouse(mouse);
        ApplyVolume(volume);

        UpdateTexts(mouse, volume);
    }

    void OnMouseChanged(float value)
    {
        ApplyMouse(value);
        PlayerPrefs.SetFloat(MOUSE_KEY, value);
        UpdateTexts(value, volumeSlider.value);
    }

    void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(VOLUME_KEY, value);
        UpdateTexts(mouseSlider.value, value);
    }

    void ApplyMouse(float value)
    {
        // myöhemmin voidaan liittää kameraan
    }

    void ApplyVolume(float value)
    {
        AudioListener.volume = value;
    }

    void UpdateTexts(float mouse, float volume)
    {
        if (mouseValueText != null)
            mouseValueText.text = mouse.ToString("0.00");

        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(volume * 100) + "%";
    }
}