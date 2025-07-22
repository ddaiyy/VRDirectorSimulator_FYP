using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MyGame.Selection;

public class XRSelectionForwarder : MonoBehaviour
{
    public XRRayInteractor rayInteractor;  // 在Inspector拖入对应的XRRayInteractor
    public XRController xrController;      // 在Inspector拖入对应的XRController，负责监听Trigger按键

    private ICustomSelectable currentHoverSelectable;    // 当前射线指向的可选中物体（hover）
    private ICustomSelectable currentSelectedSelectable; // 当前已选中的物体

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
        if (xrController != null && xrController.inputDevice.isValid)
        {
            bool triggerPressed = false;
            InputHelpers.IsPressed(xrController.inputDevice, InputHelpers.Button.Trigger, out triggerPressed);

            if (triggerPressed)
            {
                // 按下Trigger时选中当前hover的物体
                if (currentHoverSelectable != null && currentSelectedSelectable != currentHoverSelectable)
                {
                    currentSelectedSelectable?.OnDeselect();
                    currentSelectedSelectable = currentHoverSelectable;
                    currentSelectedSelectable.OnSelect();
                }
            }
            // 注意这里不在松开Trigger时取消选中，所以canvas保持显示
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
