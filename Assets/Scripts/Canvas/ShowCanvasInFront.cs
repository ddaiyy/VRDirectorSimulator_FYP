using UnityEngine;

public class ShowCanvasInFront : MonoBehaviour
{
    public GameObject targetCanvas;   // 要显示的 Canvas
    public Transform playerCamera;    // XR Origin 中的 Main Camera
    public float distance = 4f;       // 前方距离（建议 VR 用 1.5~2）
    public float verticalOffset = 0.0f; // 垂直偏移

    private bool isShowing = false;

    // 点击按钮调用
    public void ShowCanvas()
    {
        if (targetCanvas == null || playerCamera == null) return;

        isShowing = true;
        targetCanvas.SetActive(true);
        UpdateCanvasPosition();
    }

    // 点击其他按钮隐藏
    public void HideCanvas()
    {
        isShowing = false;
        targetCanvas.SetActive(false);
    }

    void Update()
    {
        if (isShowing && targetCanvas.activeSelf)
        {
            UpdateCanvasPosition();
        }
    }

    void UpdateCanvasPosition()
    {
        // 计算 UI 位置：玩家正前方 + 垂直偏移
        Vector3 forwardPos = playerCamera.position + playerCamera.forward * distance;
        forwardPos.y += verticalOffset;
        targetCanvas.transform.position = forwardPos;

        // 只旋转 Y 轴，避免 UI 跟着上下倾斜
        Vector3 lookDirection = playerCamera.position - targetCanvas.transform.position;
        lookDirection.y = 0; // 锁定 Y 轴
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            targetCanvas.transform.rotation = Quaternion.LookRotation(-lookDirection);
        }
    }
}
