using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRRayCanvasScaler : MonoBehaviour
{
    public XRRayInteractor leftHand;
    public XRRayInteractor rightHand;

    private bool leftGrabbing = false;
    private bool rightGrabbing = false;

    private Vector3 leftGrabPoint;
    private Vector3 rightGrabPoint;

    private float initialDistance;
    private Vector3 initialScale;

    public float minScale = 0.01f;
    public float maxScale = 0.1f;

    void Update()
    {
        UpdateGrabState();

        if (leftGrabbing && rightGrabbing)
        {
            float currentDistance = Vector3.Distance(leftGrabPoint, rightGrabPoint);

            if (initialDistance == 0f)
            {
                initialDistance = currentDistance;
                initialScale = transform.localScale;
            }

            float scaleFactor = currentDistance / initialDistance;
            scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale);

            transform.localScale = initialScale * scaleFactor;
        }
        else
        {
            initialDistance = 0f;
        }
    }

    void UpdateGrabState()
    {
        leftGrabbing = leftHand.TryGetCurrent3DRaycastHit(out RaycastHit leftHit) && leftHit.collider == GetComponent<Collider>();
        rightGrabbing = rightHand.TryGetCurrent3DRaycastHit(out RaycastHit rightHit) && rightHit.collider == GetComponent<Collider>();

        if (leftGrabbing)
            leftGrabPoint = leftHand.transform.position + leftHand.transform.forward * leftHit.distance;
        if (rightGrabbing)
            rightGrabPoint = rightHand.transform.position + rightHand.transform.forward * rightHit.distance;
    }

}
