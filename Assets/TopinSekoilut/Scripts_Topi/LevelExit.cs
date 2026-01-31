using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Only trigger popup if exit is actually enabled (all items collected)
            if (GameManager.Instance.IsExitEnabled())
            {
                GameManager.Instance.ShowLevelCompletePopup();
            }
            else
            {
                Debug.Log("Collect all items before exiting!");
                // Optional: Show quick message to player
                // Or play a sound effect
            }
        }
    }
}
