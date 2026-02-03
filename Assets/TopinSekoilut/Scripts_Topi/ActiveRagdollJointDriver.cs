using UnityEngine;

[DefaultExecutionOrder(-50)]
public class ActiveRagdollJointDriver : MonoBehaviour
{
    public ConfigurableJoint joint;
    public Transform animatedBone;

    // Cached bind-pose relationship
    private Quaternion _startLocalRotationRagdoll;
    private Quaternion _startLocalRotationAnimated;

    // Cached conversion into "joint space"
    private Quaternion _jointSpace;

    void Reset()
    {
        joint = GetComponent<ConfigurableJoint>();
    }

    void Awake()
    {
        if (!joint) joint = GetComponent<ConfigurableJoint>();
        if (!joint || !animatedBone || !joint.connectedBody)
        {
            enabled = false;
            return;
        }

        // Cache bind pose local rotations
        _startLocalRotationRagdoll = transform.localRotation;
        _startLocalRotationAnimated = animatedBone.localRotation;

        // Build joint-space basis from joint axis + secondary axis.
        // This is the critical piece most implementations miss.
        Vector3 right = joint.axis.normalized;
        Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;

        _jointSpace = Quaternion.LookRotation(forward, up);
    }

    void FixedUpdate()
    {
        // What rotation do we want relative to bind pose?
        // (animatedLocal * inverse(animatedBind)) gives animation delta from bind.
        Quaternion animDelta = animatedBone.localRotation * Quaternion.Inverse(_startLocalRotationAnimated);

        // Apply that delta onto ragdoll bind pose.
        Quaternion desiredLocal = animDelta * _startLocalRotationRagdoll;

        // Convert desiredLocal into joint space expected by ConfigurableJoint.targetRotation.
        // targetRotation is in joint space relative to connected body.
        Quaternion target = Quaternion.Inverse(_jointSpace) * Quaternion.Inverse(desiredLocal) * _jointSpace;

        joint.targetRotation = target;
    }
}
