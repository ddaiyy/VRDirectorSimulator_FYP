using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] public CameraController currentSelected;
    public GameObject currentCamera;
    public GameObject startCamera;
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
    

    /// <summary>
    /// 根据时间点获取应该激活的相机
    /// </summary>
    /// <param name="currentTime">当前时间点</param>
    /// <returns>应该激活的相机，如果没有则返回null</returns>
    public CameraController GetExpectedActiveCameraAtTime(float currentTime)
    {
        foreach (var track in TimelineManager.Instance.GetAllTracks())
        {
            if (track.isControlledByMaster && track.isCamera)
            {
                var expectedController = track.GetExpectedActiveCameraControllerAtTime(currentTime);
                if (expectedController != null)
                {
                    return expectedController;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 主线控制下更新相机状态 - 如果已经有相机激活，返回false
    /// </summary>
    /// <param name="cameraController">要更新的相机</param>
    /// <param name="shouldBeActive">是否应该激活</param>
    /// <param name="time">当前时间</param>
    /// <returns>是否成功更新状态</returns>
    public bool UpdateCameraStateForMasterControl(CameraController cameraController, bool shouldBeActive, float time)
    {
        if (shouldBeActive)
        {
            // 如果要激活相机，检查是否已经有其他相机激活
            if (currentSelected != null && currentSelected != cameraController)
            {
                //Debug.LogError($"[CameraManager] 主线控制：{cameraController.transform.parent.gameObject.name} 试图激活，但 {currentSelected.transform.parent.gameObject.name} 已经激活 (时间: {time:F2}s)");
                return false; // 冲突，返回false
            }
            
            // 如果是同一个相机重复激活，直接返回成功
            if (currentSelected == cameraController)
            {
                //Debug.Log($"[CameraManager] 主线控制：{cameraController.transform.parent.gameObject.name} 已经是激活状态 (时间: {time:F2}s)");
                return true;
            }
            
            // 激活相机
            SelectCamera(cameraController);
            //Debug.Log($"[CameraManager] 主线控制：{cameraController.transform.parent.gameObject.name} 激活 (时间: {time:F2}s)");
            return true;
        }
        else
        {
            // 如果要禁用相机，检查是否是当前激活的相机
            if (currentSelected == cameraController)
            {
                // 禁用当前相机
                currentSelected.DisablePreview();
                currentSelected = null;
                currentCamera = null;
                
                Debug.Log($"[CameraManager] 主线控制：{cameraController.transform.parent.gameObject.name} 禁用 (时间: {time:F2}s)");
                
                // 注意：这里不自动切换到其他相机，让其他轨道的激活逻辑来处理
                // 这样可以避免在并行执行时产生冲突
            }
            return true; // 禁用总是成功的
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
            return new List<CameraController>();
        }
        
        Debug.LogError($"[CameraManager] 时间 {currentTime:F2}s: 检测到冲突！冲突相机数量: {conflictingCameras.Count}");
        foreach (var camera in conflictingCameras)
        {
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

        if (currentSelected != null)
        {
            currentSelected.DisablePreview();
        }

        currentSelected = controller;
        currentCamera = controller.transform.parent.gameObject;
        currentSelected.EnablePreview(previewTexture);

        //Debug.Log($"Selected camera: {controller.gameObject.name}");

        FindObjectOfType<CameraFOVSlider>()?.SyncSlider(controller);
        OnSelectedCameraChanged?.Invoke(currentSelected);
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

        //  ���� Controller �������ĸ� Rig�����壩
        controller.rigRoot = camObj;

        RegisterCamera(controller);
        SelectCamera(controller); // ������Զ�ѡ�У�Ԥ���������
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

    public CameraController GetCurrentSelectedCamera()
    {
        return currentSelected;
    }

    public List<CameraController> GetAllCameras()
    {
        return cameraList;
    }
    

}