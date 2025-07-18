using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public RenderTexture previewTexture; // Ԥ����RT
    public GameObject cameraPrefab; // �����Ԥ����
    public Transform cameraSpawnPoint; // ����λ��

    private List<CameraController> cameraList = new List<CameraController>();
    private CameraController currentSelected;
    public Renderer previewPlaneRenderer;

    private void Awake()
    {
        Instance = this;
    }
    // ��CameraManager�У���ӡpreviewTexture��Ϣȷ��
    private void Start()
    {
        if (previewTexture != null)
        {
            RenderTexture.active = previewTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;
        }
        if (previewTexture == null)
        {
            Debug.LogError("previewTexture is not assigned!");
        }
        else
        {
            previewPlaneRenderer.material.mainTexture = previewTexture;
        }
    }

    public void RegisterCamera(CameraController controller)
    {
        cameraList.Add(controller);
        /*controller.DisablePreview(); // Ĭ�϶���Ԥ��*/
    }

    public void DeleteCamera(CameraController controller)
    {
        if (cameraList.Contains(controller))
        {
            if (currentSelected == controller)
            {
                currentSelected.DisablePreview();
                currentSelected = null;
            }

            cameraList.Remove(controller);

            // ������������������ṹ��������������������
            if (controller.rigRoot != null)
            {
                Destroy(controller.rigRoot);
            }
            else
            {
                // ���ˣ���� rigRoot û�����ã���ɾ������
                Destroy(controller.gameObject);
            }
        }
    }



    public void SelectCamera(CameraController controller)
    {
        // 1. �ر�֮ǰ�������
        if (currentSelected != null && currentSelected != controller)
        {
            currentSelected.DisablePreview();
        }

        // 2. �����µ������Ϊ��ǰѡ��
        currentSelected = controller;

        // 3. ����������Ⱦ�� RenderTexture
        currentSelected.EnablePreview(previewTexture);
    }


    public void AddNewCamera()
    {
        GameObject camObj = Instantiate(cameraPrefab, cameraSpawnPoint.position, cameraSpawnPoint.rotation);
        CameraController controller = camObj.GetComponentInChildren<CameraController>();

        if (controller == null)
        {
            Debug.LogError("CameraController not found on the instantiated cameraPrefab!");
            return;
        }

        //  ���� Controller �������ĸ� Rig�����壩
        controller.rigRoot = camObj;

        RegisterCamera(controller);
        SelectCamera(controller); // ������Զ�ѡ�У�Ԥ���������
    }

}
