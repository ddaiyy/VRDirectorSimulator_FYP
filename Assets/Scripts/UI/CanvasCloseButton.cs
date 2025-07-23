using UnityEngine;

public class CanvasCloseButton : MonoBehaviour
{
    // 用于在其他 UI 元素控制本 Canvas 显示状态
    public GameObject canvasRoot; // 一般就是挂这个脚本的 GameObject 本身

    private void Awake()
    {
        if (canvasRoot == null)
        {
            canvasRoot = this.gameObject; // 默认隐藏自己
        }
    }

    // 给按钮调用的函数
    public void CloseCanvas()
    {
        if (canvasRoot != null)
        {
            canvasRoot.SetActive(false);
            Debug.Log("Canvas 已关闭");
        }
        else
        {
            Debug.LogWarning("没有指定 Canvas Root");
        }
    }

    // 在无头显环境下测试用
    [ContextMenu("测试关闭 Canvas")]
    public void TestCloseCanvas()
    {
        CloseCanvas();
    }
}
