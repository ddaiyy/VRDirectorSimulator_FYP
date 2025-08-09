using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    
    public GameObject cameraPrefab; // �����Ԥ����
    public Transform cameraSpawnPoint; // ����λ��

    public delegate void SelectedCameraChangedHandler(CameraController selectedCamera);

    public event SelectedCameraChangedHandler OnSelectedCameraChanged;

    [SerializeField] public List<CameraController> cameraList = new List<CameraController>();
    [SerializeField] public CameraController currentSelected;
    
    public GameObject currentCamera;
    public GameObject startCamera;
    
    public Renderer previewPlaneRenderer;
    public RenderTexture previewTexture;
    
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
    

    /// <summary>
    /// 检测当前时间点是否有多个相机试图同时激活
    /// </summary>
    /// <param name="currentTime">当前时间点</param>
    /// <returns>冲突的相机列表，如果没有冲突返回空列表</returns>
    public List<CameraController> CheckCameraConflictAtTime(float currentTime)
    {
        List<CameraController> conflictingCameras = new List<CameraController>();
        
        //Debug.Log($"[CameraManager] 开始检测时间 {currentTime:F2}s 的相机冲突...");
        
        foreach (var track in TimelineManager.Instance.GetAllTracks())
        {
            if (track.isControlledByMaster && track.isCamera)
            {
                var expectedController = track.GetExpectedActiveCameraControllerAtTime(currentTime);
                if (expectedController != null)
                {
                    conflictingCameras.Add(expectedController);
                    //Debug.Log($"[CameraManager] 时间 {currentTime:F2}s: {track.gameObject.name} 应该激活");
                }
                else
                {
                    //Debug.Log($"[CameraManager] 时间 {currentTime:F2}s: {track.gameObject.name} 不应该激活");
                }
            }
        }
        
        // 如果只有一个或没有相机应该激活，则没有冲突
        if (conflictingCameras.Count <= 1)
        {
            //Debug.Log($"[CameraManager] 时间 {currentTime:F2}s: 没有冲突，激活相机数量: {conflictingCameras.Count}");
            return conflictingCameras;
        }
        FeedbackManager.Instance.ShowMessage("Camera conflict!", MessageType.Error);
        Debug.LogError($"[CameraManager] 时间 {currentTime:F2}s: 检测到冲突！冲突相机数量: {conflictingCameras.Count}");
        foreach (var camera in conflictingCameras)
        {
            FeedbackManager.Instance.ShowMessage("Camera conflict!", MessageType.Error);
            Debug.LogError($"[CameraManager] 冲突相机: {camera.transform.parent.gameObject.name}");
        }
        
        return conflictingCameras;
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
                    /*if (expectedController != null && controller != expectedController)
                    {
                        Debug.LogError(
                            $"[CameraManager] Master播放期间禁止手动切换相机！当前应激活：{expectedController.gameObject.name}");
                        return; // 阻止切换
                    }*/
                }
            }
        }

        if (currentSelected!=null && currentSelected !=controller)
        {
            currentSelected.DisablePreview();
        }

        currentSelected = controller;
        currentCamera = controller.transform.parent.gameObject;
        currentSelected.EnablePreview(previewTexture);

        //Debug.Log($"Selected camera: {controller.gameObject.name}");

        //FindObjectOfType<CameraFOVSlider>()?.SyncSlider(controller);
        //OnSelectedCameraChanged?.Invoke(currentSelected);
    }

    public void ClearSelectedCamera(CameraController cameraController)
    {
        if(currentSelected!=cameraController) return;
        
        // 禁用相机输出
        if (currentSelected != null)
        {
            currentSelected.DisablePreview();
        }

        currentSelected = null;
        currentCamera = null;
        
        //TODO:如果画面卡住前一个加强制
        if (previewTexture != null)
        {
            RenderTexture activeRT = RenderTexture.active;
            RenderTexture.active = previewTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = activeRT;

            previewPlaneRenderer.material.mainTexture = previewTexture;
        }
        
    } 
    
    public void AddNewCamera()
    {
        GameObject camObj = Instantiate(cameraPrefab, cameraSpawnPoint.position, cameraSpawnPoint.rotation);
        camObj.name=GetCameraNameWithIndex(cameraPrefab.name);
        CameraController controller = camObj.GetComponentInChildren<CameraController>();
        
        TimelineManager.Instance.RegisterTrack(camObj.GetComponentInChildren<TimelineTrack>());

        if (controller == null)
        {
            Debug.LogError("CameraController not found on the instantiated cameraPrefab!");
            return;
        }

        controller.rigRoot = camObj;

        RegisterCamera(controller);
        SelectCamera(controller); 
    }

    private string GetCameraNameWithIndex(string cameraPrefabName)
    {
       var allObjects = GameObject.FindObjectsOfType<Transform>(true)
                   .Select(t => t.gameObject)
                   .Where(go => go.name.StartsWith(cameraPrefabName))
                   .ToList();
               int maxIndex = 0;
       
               foreach (var obj in allObjects)
               {
                   string suffix = obj.name.Substring(cameraPrefabName.Length); // 去掉前缀
                   if (int.TryParse(suffix, out int index))
                   {
                       if (index > maxIndex)
                           maxIndex = index;
                   }
               }
       
               int nextIndex = maxIndex + 1;
               return cameraPrefabName + nextIndex;
    }

    public CameraController GetCurrentSelectedCameraController()
    {
        return currentSelected;
    }

    public List<CameraController> GetAllCameras()
    {
        return cameraList;
    }
    

}