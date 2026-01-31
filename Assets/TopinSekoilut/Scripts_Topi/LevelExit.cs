using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(playerTag))
        {
            GameManager.Instance.EnableLevelExit();
        }
    }

}
