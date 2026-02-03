using UnityEngine;

public class copyAnim : MonoBehaviour
{
    public Transform targetLimb;
    public bool mirror;

    [SerializeField] Transform animRoot;   // root of the animated copy rig (NOT ragdoll hips)

    private ConfigurableJoint configurableJoint;
    private Quaternion startRotation;

    private void Awake()
    {
        configurableJoint = GetComponent<ConfigurableJoint>();
        startRotation = transform.localRotation;
    }
    void FixedUpdate()
    {
        configurableJoint.targetRotation = Quaternion.Inverse(targetLimb.localRotation) * startRotation;
    }
}
