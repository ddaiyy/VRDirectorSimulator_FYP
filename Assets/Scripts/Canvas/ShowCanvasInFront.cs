using UnityEngine;

public class ShowCanvasInFront : MonoBehaviour
{
    public GameObject targetCanvas;   // 要显示的 Canvas
    public Transform playerCamera;    // XR Origin 中的 Main Camera
    public float distance = 8f;       // 前方距离
    public float verticalOffset = 1f; // 垂直偏移，可在 Inspector 调

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
        // 始终在玩家眼前
        Vector3 forwardPos = playerCamera.position + playerCamera.forward * distance;
        forwardPos.y += verticalOffset;

        targetCanvas.transform.position = forwardPos;
        targetCanvas.transform.rotation = playerCamera.rotation;
    }
}
