using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class CanvasSpawner : MonoBehaviour
{
    [Header("Canvas 预制体")]
    public GameObject canvasPrefab;

    private GameObject currentCanvasInstance;

    void Start()
    {
        // 确保摄像机存在
        if (Camera.main == null)
        {
            Debug.LogError("没有找到主摄像机！");
        }
    }

    // 这个函数在按钮点击时被调用
    public void ToggleCanvas()
    {
        if (currentCanvasInstance == null)
        {
            SpawnCanvasInFrontOfUser();
        }
        else
        {
            Destroy(currentCanvasInstance);
            currentCanvasInstance = null;
        }
    }

    void SpawnCanvasInFrontOfUser()
    {
        if (canvasPrefab == null || Camera.main == null) return;

        // 获取摄像机位置和方向
        Transform cam = Camera.main.transform;

        Vector3 spawnPosition = cam.position + cam.forward * 10f + Vector3.up * -0.2f; // 稍微下移一点
        Quaternion rotation = Quaternion.LookRotation(cam.forward);

        currentCanvasInstance = Instantiate(canvasPrefab, spawnPosition, rotation);
        currentCanvasInstance.SetActive(true);

        // 可选：让 Canvas 始终正对玩家（只绕 Y）
        Vector3 lookPos = new Vector3(cam.position.x, currentCanvasInstance.transform.position.y, cam.position.z);
        currentCanvasInstance.transform.LookAt(lookPos);
        currentCanvasInstance.transform.Rotate(0, 180, 0); // 反向使 UI 正对

        // 自动绑定 Close 按钮
        Button closeBtn = currentCanvasInstance.GetComponentInChildren<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(CloseCanvas);
        }
    }

    public void CloseCanvas()
    {
        if (currentCanvasInstance != null)
        {
            Destroy(currentCanvasInstance);
            currentCanvasInstance = null;
        }
    }

    [ContextMenu("测试/创建 Canvas")]
    void Debug_SpawnCanvas()
    {
        if (currentCanvasInstance == null)
            SpawnCanvasInFrontOfUser();
        else
            Debug.LogWarning("Canvas 已存在，跳过创建");
    }

    [ContextMenu("测试/销毁 Canvas")]
    void Debug_DestroyCanvas()
    {
        if (currentCanvasInstance != null)
            CloseCanvas();
        else
            Debug.LogWarning("没有 Canvas 可销毁");
    }
}
