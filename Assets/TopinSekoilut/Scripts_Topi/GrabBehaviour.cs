using UnityEngine;

public class GrabBehaviour : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] LayerMask grabbableLayers;

    GameObject grabbedObject;
    [SerializeField] Rigidbody rb;
    FixedJoint joint;

    public int isLeftOrRight;
    public bool alreadyGrabbed = false;

    private InventoryService _inv; // ✅ NEW

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        _inv = InventoryService.Instance;
        if (_inv == null)
            Debug.LogWarning("GrabBehaviour: InventoryService not found (auto-pickup will not work).");
    }

    private void Update()
    {
        if (Input.GetMouseButton(isLeftOrRight))
        {
            if (animator != null)
                animator.SetBool(isLeftOrRight == 0 ? "isHandUp_Left" : "isHandUp_Right", true);

            if (grabbedObject != null && joint == null)
            {
                if (grabbedObject.GetComponent<FixedJoint>() != null)
                {
                    Debug.LogWarning("GrabBehaviour: Trying to grab an object that is already stuck in another hand.");
                    return;
                }

                joint = grabbedObject.AddComponent<FixedJoint>();
                joint.connectedBody = rb;
                joint.breakForce = 1000f;
                alreadyGrabbed = true;
            }
        }

        if (Input.GetMouseButtonUp(isLeftOrRight))
        {
            if (animator != null)
                animator.SetBool(isLeftOrRight == 0 ? "isHandUp_Left" : "isHandUp_Right", false);

            Release();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) Keep existing grab selection
        if (((1 << other.gameObject.layer) & grabbableLayers) != 0 && !alreadyGrabbed)
        {
            grabbedObject = other.gameObject;
        }

        // 2) Auto-pickup into inventory if item has PickUpItem
        var pickupItem = other.GetComponent<PickUpItem>();
        if (pickupItem != null && !pickupItem.pickedUp)
        {
            if (_inv == null) _inv = InventoryService.Instance;
            if (_inv == null) return;

            pickupItem.pickedUp = true;
            _inv.AddItem(pickupItem.itemId, pickupItem.amount);

            // Optional: show pickup feed if you use InventoryUI_TMP
            var ui = Object.FindFirstObjectByType<InventoryUI>();
            if (ui != null) ui.ShowPickup(pickupItem.itemId);

            if (pickupItem.destroyOnPickup)
            {
                // If we were about to grab it, clear references first
                if (grabbedObject == other.gameObject && joint == null)
                {
                    grabbedObject = null;
                    alreadyGrabbed = false;
                }

                Destroy(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == grabbedObject && joint == null)
        {
            grabbedObject = null;
            alreadyGrabbed = false;
        }
    }

    private void Release()
    {
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }

        grabbedObject = null;
        alreadyGrabbed = false;
    }
}
