using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ShowTooltipOnHover : MonoBehaviour
{
    public GameObject tooltipCanvas; // 拖入 Tooltip Canvas 对象

    private void Awake()
    {
        if (tooltipCanvas != null)
            tooltipCanvas.SetActive(false);
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (tooltipCanvas != null)
            tooltipCanvas.SetActive(true);
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (tooltipCanvas != null)
            tooltipCanvas.SetActive(false);
    }

    // 测试方法：在 Inspector 上右键调用
    [ContextMenu("Test Show Tooltip")]
    public void TestShowTooltip()
    {
        if (tooltipCanvas != null)
        {
            tooltipCanvas.SetActive(true);
            Debug.Log("Test Show Tooltip");
        }
    }

    [ContextMenu("Test Hide Tooltip")]
    public void TestHideTooltip()
    {
        if (tooltipCanvas != null)
        {
            tooltipCanvas.SetActive(false);
            Debug.Log("Test Hide Tooltip");
        }
    }
}
