using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventoryService inventoryService;

    [Header("Bottle HUD")]
    [SerializeField] private string bottleGroupId = "beer_can";
    [SerializeField] private Image bottleIconImage;
    [SerializeField] private TMP_Text bottleCountText;

    [Header("Stamina HUD")]
    [SerializeField] private RectTransform staminaGreen;
    [SerializeField] private float maxGreenHeight = 120f;
    [SerializeField] private float staminaSmoothSpeed = 10f;

    private float displayedNormalized = 1f;

    private void Start()
    {
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (inventoryService == null)
            inventoryService = InventoryService.Instance;

        if (inventoryService != null)
            inventoryService.OnChanged += RefreshBottleHUD;

        RefreshBottleHUD();
        ForceRefreshStaminaHUD();
    }

    private void OnDestroy()
    {
        if (inventoryService != null)
            inventoryService.OnChanged -= RefreshBottleHUD;
    }

    private void Update()
    {
        RefreshStaminaHUD();
    }

    private void RefreshBottleHUD()
    {
        if (inventoryService == null) return;

        int count = inventoryService.GetCountByGroup(bottleGroupId);
        Sprite icon = inventoryService.GetGroupIcon(bottleGroupId);

        if (bottleCountText != null)
            bottleCountText.text = count.ToString();

        if (bottleIconImage != null && icon != null)
            bottleIconImage.sprite = icon;
    }

    private void ForceRefreshStaminaHUD()
    {
        if (playerController == null || staminaGreen == null) return;

        displayedNormalized = playerController.StaminaNormalized;
        ApplyStaminaHeight(displayedNormalized);
    }

    private void RefreshStaminaHUD()
    {
        if (playerController == null || staminaGreen == null) return;

        float target = playerController.StaminaNormalized;
        displayedNormalized = Mathf.Lerp(displayedNormalized, target, Time.deltaTime * staminaSmoothSpeed);
        ApplyStaminaHeight(displayedNormalized);
    }

    private void ApplyStaminaHeight(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        Vector2 size = staminaGreen.sizeDelta;
        size.y = maxGreenHeight * normalized;
        staminaGreen.sizeDelta = size;
    }
}