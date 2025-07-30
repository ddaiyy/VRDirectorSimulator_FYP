using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ConsoleTriggerCanvas : MonoBehaviour
{
    public GameObject targetCanvas; // 拖入你的 UI Canvas

    private XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnSelect);
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        ToggleCanvas();
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(OnSelect);
    }

    // 👉 切换 Canvas 显示/隐藏
    private void ToggleCanvas()
    {
        if (targetCanvas != null)
        {
            bool newState = !targetCanvas.activeSelf;
            targetCanvas.SetActive(newState);
            Debug.Log("Canvas 状态切换为: " + newState);
        }
        else
        {
            Debug.LogWarning("未设置 targetCanvas！");
        }
    }

    // 👉 在 Inspector 中右键点击脚本标题调用测试
    [ContextMenu("测试切换 Canvas 显示状态")]
    private void TestToggleCanvas()
    {
        ToggleCanvas();
    }
}
