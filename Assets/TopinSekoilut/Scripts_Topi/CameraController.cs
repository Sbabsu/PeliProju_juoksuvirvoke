using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform root;

    float mouseX, mouseY;

    [SerializeField] ConfigurableJoint hipJoint, chestJoint;

    private void FixedUpdate()
    {
        CamControl();
    }


    void CamControl()
    {
        mouseX += Input.GetAxis("Mouse X");

        Quaternion rootRotation = Quaternion.Euler(0, mouseX, 0);

        root.rotation = rootRotation;

        hipJoint.targetRotation = Quaternion.Euler(0, -mouseX, 0);
        chestJoint.targetRotation = Quaternion.Euler(0, -mouseX, 0);
    }
}
