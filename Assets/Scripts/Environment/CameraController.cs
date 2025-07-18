using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject worldCanvas; // ��ťCanvas�������������Prefab��
    public Camera mainPlayerCamera;

    private Camera cam;
    [HideInInspector]
    public GameObject rigRoot; // ��ӣ����� VirtualCameraRig ������
    private void Awake()
    {
        if (mainPlayerCamera == null)
        {
            mainPlayerCamera = Camera.main;
        }

        cam = GetComponent<Camera>();

        // �������������Ĭ�ϲ�д�� RT����ֹ���� bug
        cam.enabled = true;
        cam.targetTexture = null;

        if (worldCanvas != null)
            worldCanvas.SetActive(false); // Ĭ�����ذ�ťUI
    }

    private void Start()
    {
        // �����������������
        mainPlayerCamera.enabled = true;
        mainPlayerCamera.targetTexture = null;

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.RegisterCamera(this);
        }
        else
        {
            Debug.LogError("CameraManager.Instance is null in Start!");
        }
    }

   
    public void EnablePreview(RenderTexture rt)
    {
        cam.targetTexture = rt;

        // ��ֹ���������Ļ��ֻ������ targetTexture �������������
        if (rt != null)
        {
            cam.enabled = true;
        }

        if (worldCanvas != null)
            worldCanvas.SetActive(true);
    }


    public void DisablePreview()
    {
        cam.targetTexture = null;
        cam.enabled = false;

        if (worldCanvas != null)
            worldCanvas.SetActive(false);
    }


    public void SelectThisCamera()
    {
        CameraManager.Instance.SelectCamera(this);
    }

    public void DeleteThisCamera()
    {
        CameraManager.Instance.DeleteCamera(this);
    }
}
