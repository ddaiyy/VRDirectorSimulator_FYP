using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRCanvasToggleButton : MonoBehaviour
{
    [Header("������� CameraController �ű�����")]
    public CameraController cameraController;

    private XRBaseInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelected);
        }
        else
        {
            Debug.LogWarning("δ�ҵ� XRBaseInteractable ����������Ƿ������ XR Grab Interactable");
        }
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelected);
        }
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        ToggleCanvas();
    }

    [ContextMenu("���� ToggleCanvas")]
    public void ToggleCanvas()
    {
        if (cameraController != null)
        {
            cameraController.ToggleWorldCanvas();
        }
        else
        {
            Debug.LogWarning("cameraController δ�󶨣�");
        }
    }
}
