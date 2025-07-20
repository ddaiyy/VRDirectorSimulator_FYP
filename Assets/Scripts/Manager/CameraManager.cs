using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public RenderTexture previewTexture; // Ԥ����RT
    public GameObject cameraPrefab; // �����Ԥ����
    public Transform cameraSpawnPoint; // ����λ��
    public delegate void SelectedCameraChangedHandler(CameraController selectedCamera);
    public event SelectedCameraChangedHandler OnSelectedCameraChanged;

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
        TimelineManager.Instance.UnregisterTrack(controller.GameObject().GetComponentInChildren<TimelineTrack>());
        if (cameraList.Contains(controller))
        {
            bool wasCurrent = (currentSelected == controller);

            if (wasCurrent)
            {
                currentSelected.DisablePreview();
                currentSelected = null;
            }
            
            cameraList.Remove(controller);

            //  ɾ������
            if (controller.rigRoot != null)
            {
                Destroy(controller.rigRoot);
            }
            else
            {
                Destroy(controller.gameObject);
            }

            // ���ɾ���ǵ�ǰѡ�У����һ���������������Զ��л�����һ��
            if (wasCurrent && cameraList.Count > 0)
            {
                SelectCamera(cameraList[0]); // ��Ҳ����ѡ��һ����cameraList[0]
            }

            // ���û������ˣ������Ԥ������
            if (cameraList.Count == 0 && previewPlaneRenderer != null)
            {
                previewPlaneRenderer.material.mainTexture = null;
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

        // ֪ͨ UI ��ǰ����ֵ
        FindObjectOfType<CameraFOVSlider>()?.SyncSlider(controller);

        // 4. �����¼���֪ͨUI�ȶ�����
        OnSelectedCameraChanged?.Invoke(currentSelected);
        
    
}


    public void AddNewCamera()
    {
        GameObject camObj = Instantiate(cameraPrefab, cameraSpawnPoint.position, cameraSpawnPoint.rotation);
        CameraController controller = camObj.GetComponentInChildren<CameraController>();
        
        TimelineManager.Instance.RegisterTrack(camObj.GetComponentInChildren<TimelineTrack>());

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

    public CameraController GetCurrentSelectedCamera()
    {
        return currentSelected;
    }

    public List<CameraController> GetAllCameras()
    {
        return cameraList;
    }

}
