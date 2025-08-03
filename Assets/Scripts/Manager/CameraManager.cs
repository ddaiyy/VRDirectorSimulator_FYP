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

    [SerializeField] public List<CameraController> cameraList = new List<CameraController>();
    [SerializeField] private CameraController currentSelected;
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
        // 检查是否有Master控制
        if (TimelineManager.Instance.masterTrack != null && TimelineManager.Instance.masterTrack.isPlaying)
        {
            // 检查当前所有受控轨道，是否允许切换
            foreach (var track in TimelineManager.Instance.GetAllTracks())
            {
                if (track.isControlledByMaster && track.isCamera)
                {
                    var expectedController =
                        track.GetExpectedActiveCameraControllerAtTime(TimelineManager.Instance.masterTrack.currentTime);
                    if (expectedController != null && controller != expectedController)
                    {
                        Debug.LogError(
                            $"[CameraManager] Master播放期间禁止手动切换相机！当前应激活：{expectedController.gameObject.name}");
                        return; // 阻止切换
                    }
                }
            }
        }

        if (currentSelected != null)
        {
            currentSelected.DisablePreview();
        }

        currentSelected = controller;

        currentSelected.EnablePreview(previewTexture);

        Debug.Log($"Selected camera: {controller.gameObject.name}");

        FindObjectOfType<CameraFOVSlider>()?.SyncSlider(controller);
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