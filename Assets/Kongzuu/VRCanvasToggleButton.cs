using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRCanvasToggleButton : MonoBehaviour
{
    [Header("挂载你的 CameraController 脚本对象")]
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
            Debug.LogWarning("未找到 XRBaseInteractable 组件，请检查是否添加了 XR Grab Interactable");
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

    [ContextMenu("测试 ToggleCanvas")]
    public void ToggleCanvas()
    {
        if (cameraController != null)
        {
            cameraController.ToggleWorldCanvas();
        }
        else
        {
            Debug.LogWarning("cameraController 未绑定！");
        }
    }
}
