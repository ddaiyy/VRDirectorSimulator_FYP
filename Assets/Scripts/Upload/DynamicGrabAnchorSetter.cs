using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DynamicGrabAnchorSetter : MonoBehaviour
{
    private XRGrabInteractable grab;
    private Transform dynamicAnchor;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrabStart);
    }

    void OnGrabStart(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRRayInteractor rayInteractor)
        {
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                // 创建临时锚点
                if (dynamicAnchor == null)
                {
                    GameObject anchor = new GameObject("DynamicGrabAnchor");
                    anchor.transform.SetParent(transform);
                    dynamicAnchor = anchor.transform;
                }

                dynamicAnchor.position = hit.point;
                dynamicAnchor.rotation = Quaternion.LookRotation(rayInteractor.transform.forward, Vector3.up);
                grab.attachTransform = dynamicAnchor;
            }
        }
    }
}
