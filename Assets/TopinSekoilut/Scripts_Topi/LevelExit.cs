using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";
    [SerializeField] ReceiptUI_TMP receiptUI;

    void Awake()
    {
        if (receiptUI == null)
            receiptUI = FindObjectOfType<ReceiptUI_TMP>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        // hyväksy osuma myös ragdollin lapsiin
        bool hitPlayer =
            other.CompareTag(playerTag) ||
            (other.transform.root != null && other.transform.root.CompareTag(playerTag));

        if (!hitPlayer) return;

        if (receiptUI == null)
        {
            Debug.LogError("LevelExit: ReceiptUI_TMP puuttuu!");
            return;
        }

        receiptUI.OpenLevelCompleteReceipt();
    }
}
