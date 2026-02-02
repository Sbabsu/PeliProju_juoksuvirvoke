using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickupFeedUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text countText;

    [Header("Behavior")]
    public float visibleSeconds = 2.0f;

    private float timer;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (panel == null) return;
        if (!panel.activeSelf) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            panel.SetActive(false);
    }

    public void Show(string displayName, int totalCount, Sprite icon = null)
    {
        if (panel == null) return;

        panel.SetActive(true);
        timer = visibleSeconds;

        if (nameText != null)
            nameText.text = displayName;

        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = icon;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        if (countText != null)
            countText.text = totalCount > 1 ? $"x{totalCount}" : "";
    }

}
