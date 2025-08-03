using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class CanvasSpawner : MonoBehaviour
{
    [Header("Canvas Ԥ����")]
    public GameObject canvasPrefab;

    private GameObject currentCanvasInstance;

    void Start()
    {
        // ȷ�����������
        if (Camera.main == null)
        {
            Debug.LogError("û���ҵ����������");
        }
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

        Vector3 spawnPosition = cam.position + cam.forward * 10f + Vector3.up * -0.2f; // ��΢����һ��
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
