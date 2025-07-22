using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MyGame.Selection;

public class XRSelectionForwarder : MonoBehaviour
{
    public XRRayInteractor rayInteractor;  // ��Inspector�����Ӧ��XRRayInteractor
    public XRController xrController;      // ��Inspector�����Ӧ��XRController���������Trigger����

    private ICustomSelectable currentHoverSelectable;    // ��ǰ����ָ��Ŀ�ѡ�����壨hover��
    private ICustomSelectable currentSelectedSelectable; // ��ǰ��ѡ�е�����

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
                // ����Triggerʱѡ�е�ǰhover������
                if (currentHoverSelectable != null && currentSelectedSelectable != currentHoverSelectable)
                {
                    currentSelectedSelectable?.OnDeselect();
                    currentSelectedSelectable = currentHoverSelectable;
                    currentSelectedSelectable.OnSelect();
                }
            }
            // ע�����ﲻ���ɿ�Triggerʱȡ��ѡ�У�����canvas������ʾ
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
