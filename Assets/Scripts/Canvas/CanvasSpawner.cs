using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class CanvasSpawner : MonoBehaviour
{
    [Header("Canvas Ԥ����")]
    public GameObject canvasPrefab;

    [Header("Canvas 位置设置")]
    public float distanceFromCamera = 6f;
    public float verticalOffset = -0.2f;

    private GameObject currentCanvasInstance;

    public GameObject clikOnCanvas;
    public float heightOffset = 0f; // 高度偏移
    public float distance = 3f; // Canvas 距离相机的距离
    void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("没有找到主相机");
        }
    }

    public void ClikOnCanvas()
    {
        // 切换激活状态
        bool newActiveState = !clikOnCanvas.activeSelf;
        clikOnCanvas.SetActive(newActiveState);

        // 如果刚刚激活，移动到用户眼前
        if (newActiveState && Camera.main != null)
        {
            Transform cam = Camera.main.transform;

            // 计算位置：相机前 distance 米 + 高度偏移
            Vector3 newPos = cam.position + cam.forward * distance;
            newPos.y += heightOffset;

            clikOnCanvas.transform.position = newPos;

            // 面向相机
            clikOnCanvas.transform.LookAt(cam);
            clikOnCanvas.transform.Rotate(0, 180f, 0);
        }
    }
    
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

        // ��ȡ�����λ�úͷ���
        Transform cam = Camera.main.transform;

        Vector3 spawnPosition = cam.position + cam.forward * distanceFromCamera + Vector3.up * verticalOffset; // ��΢����һ��
        Quaternion rotation = Quaternion.LookRotation(cam.forward);

        currentCanvasInstance = Instantiate(canvasPrefab, spawnPosition, rotation);
        currentCanvasInstance.SetActive(true);

        // ��ѡ���� Canvas ʼ��������ң�ֻ�� Y��
        Vector3 lookPos = new Vector3(cam.position.x, currentCanvasInstance.transform.position.y, cam.position.z);
        currentCanvasInstance.transform.LookAt(lookPos);
        currentCanvasInstance.transform.Rotate(0, 180, 0); // ����ʹ UI ����

        // �Զ��� Close ��ť
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

    [ContextMenu("����/���� Canvas")]
    void Debug_SpawnCanvas()
    {
        if (currentCanvasInstance == null)
            SpawnCanvasInFrontOfUser();
        else
            Debug.LogWarning("Canvas �Ѵ��ڣ���������");
    }

    [ContextMenu("����/���� Canvas")]
    void Debug_DestroyCanvas()
    {
        if (currentCanvasInstance != null)
            CloseCanvas();
        else
            Debug.LogWarning("û�� Canvas ������");
    }
}
