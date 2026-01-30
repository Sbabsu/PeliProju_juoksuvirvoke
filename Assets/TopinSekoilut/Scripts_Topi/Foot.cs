using UnityEngine;

public class Foot : MonoBehaviour
{
    public static bool IsGrounded { get; private set; } //any script can check if the player is grounded
                                                        //by reading Foot.IsGrounded.

    [SerializeField] LayerMask groundMask;

    private int _groundContacts;

    private void OnCollisionEnter(Collision collision)
    {
        if (IsGroundLayer(collision.gameObject))
        {
            _groundContacts++;
            IsGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (IsGroundLayer(collision.gameObject))
        {
            _groundContacts--;
            IsGrounded = _groundContacts > 0;
        }
    }

    private bool IsGroundLayer(GameObject obj)
    {
        return (groundMask & (1 << obj.layer)) != 0;
    }
}
