using UnityEngine;

public class CanvasCloser : MonoBehaviour
{
    // 要关闭的 Canvas 对象（在 Inspector 中拖进来）
    public GameObject canvasToClose;

    // 在按钮的 OnClick() 里绑定这个方法
    public void CloseCanvas()
    {
        if (canvasToClose != null)
        {
            Destroy(canvasToClose); // 或者 canvasToClose.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Canvas 未设置！");
        }
    }
}
