using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MyGame.Selection;

public class XRSelectionForwarder : MonoBehaviour
{
    public XRRayInteractor rayInteractor;  // ÔÚInspectorÍÏÈë¶ÔÓ¦µÄXRRayInteractor
    public ActionBasedController xrController;


    private ICustomSelectable currentHoverSelectable;    // µ±Ç°ÉäÏßÖ¸ÏòµÄ¿ÉÑ¡ÖÐÎïÌå£¨hover£©
    private ICustomSelectable currentSelectedSelectable; // µ±Ç°ÒÑÑ¡ÖÐµÄÎïÌå

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
        if (xrController != null && xrController.activateActionValue.action != null)
        {
            float triggerValue = xrController.activateActionValue.action.ReadValue<float>();

            if (triggerValue > 0.1f)
            {
                Debug.Log("Trigger Pressed (activateAction)");

                if (currentHoverSelectable != null)
                {
                    if (currentHoverSelectable is MonoBehaviour mb)
                    {
                        var cameraController = mb.GetComponent<CameraController>();
                        var propSelectable = mb.GetComponent<PropSelectable>();

                        bool isCamera = (cameraController != null);
                        bool isProp = (propSelectable != null);

                        if (currentSelectedSelectable != currentHoverSelectable)
                        {
                            // 切换选中，取消之前的选中
                            currentSelectedSelectable?.OnDeselect();
                            currentSelectedSelectable = currentHoverSelectable;
                            currentSelectedSelectable.OnSelect();
                        }
                        else
                        {
                            // 点击同一个对象，再次调用 OnSelect 来刷新预览（特别是摄像机）
                            currentSelectedSelectable.OnSelect();
                        }
                    }
                    else
                    {
                        // 非MonoBehaviour也直接调用OnSelect
                        if (currentSelectedSelectable != currentHoverSelectable)
                        {
                            currentSelectedSelectable?.OnDeselect();
                            currentSelectedSelectable = currentHoverSelectable;
                            currentSelectedSelectable.OnSelect();
                        }
                        else
                        {
                            currentSelectedSelectable.OnSelect();
                        }
                    }
                }
            }
        }
    }





    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        // 先找自己
        var selectable = args.interactableObject.transform.GetComponent<ICustomSelectable>();

        // 如果没找到，试试找子物体
        if (selectable == null)
            selectable = args.interactableObject.transform.GetComponentInChildren<ICustomSelectable>();

        if (selectable != null)
        {
            currentHoverSelectable = selectable;
            Debug.Log($"Hover进入了 {selectable}");
        }
    }


    private void OnHoverExited(HoverExitEventArgs args)
    {
        var selectable = args.interactableObject.transform.GetComponent<ICustomSelectable>();
        if (selectable == null)
            selectable = args.interactableObject.transform.GetComponentInChildren<ICustomSelectable>();

        if (selectable == currentHoverSelectable)
        {
            currentHoverSelectable = null;
        }
    }

}
