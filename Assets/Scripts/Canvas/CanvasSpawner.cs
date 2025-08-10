using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class CanvasSpawner : MonoBehaviour
{
    [Header("Canvas Ԥ����")]
    public GameObject canvasPrefab;

    [Header("Canvas 位置设置")]
    public float distanceFromCamera = 8f;
    public float verticalOffset = -0.2f;

    private GameObject currentCanvasInstance;

    public GameObject clikOnCanvas;
    void Start()
    {
        // ȷ�����������
        if (Camera.main == null)
        {
            Debug.LogError("û���ҵ����������");
        }
    }

    public void ClikOnCanvas()
    {
        clikOnCanvas.SetActive(!clikOnCanvas.activeSelf);
    }
    // ��������ڰ�ť���ʱ������
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
