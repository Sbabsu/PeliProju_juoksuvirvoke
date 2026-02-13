using UnityEngine;

public class GrabBehaviourSpring : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator animator;
    [SerializeField] LayerMask grabbableLayers;

    [Header("Hand Rigidbody (this hand)")]
    [SerializeField] Rigidbody rb;

    [Header("SpringJoint tuning (FixedJoint-like)")]
    [Tooltip("How stiff the joint is. Higher = more 'stuck' like FixedJoint.")]
    [SerializeField] float spring = 50000f;

    [Tooltip("Damping to remove wobble. Raise if it jiggles.")]
    [SerializeField] float damper = 5000f;

    [Tooltip("0 = no slack (most FixedJoint-like).")]
    [SerializeField] float maxDistance = 0f;

    [Tooltip("Makes the held object feel heavier while held (restored on release).")]
    [SerializeField] float heldExtraDrag = 1.5f;
    [SerializeField] float heldExtraAngularDrag = 1.5f;

    [Header("Stability (recommended)")]
    [Tooltip("Scales the held object's mass for the joint solver. Lower = more stable.")]
    [SerializeField] float heldMassScale = 1f;

    [Tooltip("Scales the hand's mass for the joint solver. Higher = hand dominates, less wobble.")]
    [SerializeField] float handMassScale = 50f;

    [Tooltip("Optional safety break. Set very high to basically never break.")]
    [SerializeField] float breakForce = 1000f;
    [SerializeField] float breakTorque = 1000f;

    GameObject grabbedObject;
    Rigidbody grabbedRb;
    SpringJoint grabJoint;

    public int isLeftOrRight;
    public bool alreadyGrabbed = false;

    private InventoryService _inv;

    // restore values on release
    float originalGrabbedDrag;
    float originalGrabbedAngularDrag;
    float originalHandMaxAngVel;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        originalHandMaxAngVel = rb.maxAngularVelocity;

        _inv = InventoryService.Instance;
        if (_inv == null)
            Debug.LogWarning("GrabBehaviourSpring: InventoryService not found (auto-pickup will not work).");
    }

    private void Update()
    {
        if (Input.GetMouseButton(isLeftOrRight))
        {
            if (animator != null)
                animator.SetBool(isLeftOrRight == 0 ? "isHandUp_Left" : "isHandUp_Right", true);

            if (!alreadyGrabbed && grabbedObject != null && grabJoint == null)
                TryGrab(grabbedObject);
        }

        if (Input.GetMouseButtonUp(isLeftOrRight))
        {
            if (animator != null)
                animator.SetBool(isLeftOrRight == 0 ? "isHandUp_Left" : "isHandUp_Right", false);

            Release();
        }
    }

    private void TryGrab(GameObject target)
    {
        // Prevent double grabbing (any joint)
        if (target.GetComponent<Joint>() != null)
        {
            Debug.LogWarning("GrabBehaviourSpring: Target already has a Joint (grabbed by something else).");
            return;
        }

        var trb = target.GetComponent<Rigidbody>();
        if (trb == null)
        {
            Debug.LogWarning("GrabBehaviourSpring: Tried to grab something without a Rigidbody.");
            return;
        }

        grabbedObject = target;
        grabbedRb = trb;

        // Save & apply "heavier feel"
        originalGrabbedDrag = grabbedRb.linearDamping;
        originalGrabbedAngularDrag = grabbedRb.angularDamping;

        grabbedRb.linearDamping = originalGrabbedDrag + heldExtraDrag;
        grabbedRb.angularDamping = originalGrabbedAngularDrag + heldExtraAngularDrag;

        // Create SpringJoint on the grabbed object (like your FixedJoint version)
        grabJoint = grabbedObject.AddComponent<SpringJoint>();
        grabJoint.connectedBody = rb;

        // IMPORTANT: we want a "stuck" grab point, not auto centers
        grabJoint.autoConfigureConnectedAnchor = false;

        Vector3 worldGrabPoint = transform.position;

        // Anchor on the object where the hand touched
        grabJoint.anchor = grabbedObject.transform.InverseTransformPoint(worldGrabPoint);

        // Anchor on the hand where the touch happened
        grabJoint.connectedAnchor = rb.transform.InverseTransformPoint(worldGrabPoint);

        // Make it FixedJoint-like: no slack
        grabJoint.minDistance = 0f;
        grabJoint.maxDistance = maxDistance; // set to 0 for most "normal" feel

        // Stiff + damped
        grabJoint.spring = spring;
        grabJoint.damper = damper;

        // Stability: make the hand dominate the constraint solving
        grabJoint.massScale = heldMassScale;
        grabJoint.connectedMassScale = handMassScale;

        // Break settings similar to your FixedJoint
        grabJoint.breakForce = breakForce;
        grabJoint.breakTorque = breakTorque;

        // Optional: helps reduce weird collisions between hand and object
        grabJoint.enableCollision = false;
        grabJoint.enablePreprocessing = true;

        // Prevent hand from spinning wildly
        rb.maxAngularVelocity = 20f;

        alreadyGrabbed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) Grab selection
        if (((1 << other.gameObject.layer) & grabbableLayers) != 0 && !alreadyGrabbed)
        {
            grabbedObject = other.gameObject;
        }

        // 2) Auto-pickup
        var pickupItem = other.GetComponent<PickUpItem>();
        if (pickupItem != null && !pickupItem.pickedUp)
        {
            if (_inv == null) _inv = InventoryService.Instance;
            if (_inv == null) return;

            pickupItem.pickedUp = true;
            _inv.AddItem(pickupItem.itemId, pickupItem.amount);

            var ui = Object.FindFirstObjectByType<InventoryUI>();
            if (ui != null) ui.ShowPickup(pickupItem.itemId);

            if (pickupItem.destroyOnPickup)
            {
                if (grabbedObject == other.gameObject && grabJoint == null)
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
        if (other.gameObject == grabbedObject && grabJoint == null && !alreadyGrabbed)
            grabbedObject = null;
    }

    private void Release()
    {
        if (grabJoint != null)
        {
            Destroy(grabJoint);
            grabJoint = null;
        }

        if (grabbedRb != null)
        {
            grabbedRb.linearDamping = originalGrabbedDrag;
            grabbedRb.angularDamping = originalGrabbedAngularDrag;
        }

        rb.maxAngularVelocity = originalHandMaxAngVel;

        grabbedObject = null;
        grabbedRb = null;
        alreadyGrabbed = false;
    }

    // If the joint breaks, clean up state
    private void OnJointBreak(float breakForce)
    {
        Release();
    }
}
