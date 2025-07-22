using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MyGame.Selection;

public class XRSelectionForwarder : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public XRController xrController;  // 要在Inspector拖入XR Controller组件

    private ICustomSelectable currentHoverSelectable;
    private ICustomSelectable currentSelectedSelectable;

    private void OnEnable()
    {
        rayInteractor.hoverEntered.AddListener(OnHoverEntered);
        rayInteractor.hoverExited.AddListener(OnHoverExited);
    }

    private void OnDisable()
    {
        rayInteractor.hoverEntered.RemoveListener(OnHoverEntered);
        rayInteractor.hoverExited.RemoveListener(OnHoverExited);
    }

    private void Update()
    {
        bool isTriggerPressed = false;
        if (xrController != null && xrController.inputDevice.isValid)
        {
            InputHelpers.IsPressed(xrController.inputDevice, InputHelpers.Button.Trigger, out isTriggerPressed, 0.1f);
        }

        if (isTriggerPressed)
        {
            if (currentHoverSelectable != null && currentSelectedSelectable != currentHoverSelectable)
            {
                currentSelectedSelectable?.OnDeselect();

                currentSelectedSelectable = currentHoverSelectable;
                currentSelectedSelectable.OnSelect();
            }
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        var selectable = args.interactableObject.transform.GetComponent<ICustomSelectable>();
        if (selectable != null)
        {
            currentHoverSelectable = selectable;
        }
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        var selectable = args.interactableObject.transform.GetComponent<ICustomSelectable>();
        if (selectable == currentHoverSelectable)
        {
            currentHoverSelectable = null;
        }
    }
}
