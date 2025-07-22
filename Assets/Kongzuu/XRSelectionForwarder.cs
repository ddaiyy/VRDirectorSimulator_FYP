using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MyGame.Selection;

public class XRSelectionForwarder : MonoBehaviour
{
    public XRRayInteractor rayInteractor;
    public XRBaseController xrController; // �������ֱ�����������

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
            // ��� Trigger �Ƿ���
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
            // ����㲻���Զ�ȡ��ѡ�У�����ע�͵���һ��
            currentSelectable.OnDeselect();
            currentSelectable = null;
        }

        currentHovered = null;
    }
}
