using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRDoubleHandScaler : XRGrabInteractable
{
    private XRBaseInteractor firstInteractor = null;
    private XRBaseInteractor secondInteractor = null;

    private float initialDistance;
    private Vector3 initialScale;

    public float minScale = 0.5f;
    public float maxScale = 3f;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (firstInteractor == null)
        {
            firstInteractor = args.interactorObject as XRBaseInteractor;
        }
        else if (secondInteractor == null)
        {
            secondInteractor = args.interactorObject as XRBaseInteractor;

            // 开始双手缩放
            initialDistance = Vector3.Distance(firstInteractor.transform.position, secondInteractor.transform.position);
            initialScale = transform.localScale;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (args.interactorObject == firstInteractor)
        {
            firstInteractor = secondInteractor;
            secondInteractor = null;
        }
        else if (args.interactorObject == secondInteractor)
        {
            secondInteractor = null;
        }
    }

    public void Update()
    {
        if (firstInteractor != null && secondInteractor != null)
        {
            float currentDistance = Vector3.Distance(firstInteractor.transform.position, secondInteractor.transform.position);
            float scaleFactor = currentDistance / initialDistance;

            Vector3 targetScale = initialScale * scaleFactor;
            targetScale.x = Mathf.Clamp(targetScale.x, minScale, maxScale);
            targetScale.y = Mathf.Clamp(targetScale.y, minScale, maxScale);
            targetScale.z = Mathf.Clamp(targetScale.z, minScale, maxScale);

            transform.localScale = targetScale;
        }
    }
}
