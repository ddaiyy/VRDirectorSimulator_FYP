using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MyGame.Selection;

public class XRSelectionForwarder : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public XRBaseController xrController; // 新增：手柄控制器引用

    private ICustomSelectable currentSelectable;
    private IXRInteractable currentHovered;

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
        if (currentHovered != null && xrController != null)
        {
            // 检查 Trigger 是否按下
            if (xrController.selectInteractionState.activatedThisFrame)
            {
                var selectable = currentHovered.transform.GetComponent<ICustomSelectable>();
                if (selectable != null && selectable != currentSelectable)
                {
                    currentSelectable?.OnDeselect();
                    currentSelectable = selectable;
                    currentSelectable.OnSelect();
                }
            }
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        currentHovered = args.interactableObject;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        var selectable = args.interactableObject.transform.GetComponent<ICustomSelectable>();
        if (selectable != null && selectable == currentSelectable)
        {
            // 如果你不想自动取消选中，可以注释掉下一行
            currentSelectable.OnDeselect();
            currentSelectable = null;
        }

        currentHovered = null;
    }
}
