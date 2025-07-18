using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public RenderTexture previewTexture; // 预览用RT
    public GameObject cameraPrefab; // 摄像机预制体
    public Transform cameraSpawnPoint; // 生成位置

    private List<CameraController> cameraList = new List<CameraController>();
    private CameraController currentSelected;
    public Renderer previewPlaneRenderer;

    private void Awake()
    {
        Instance = this;
    }
    // 在CameraManager中，打印previewTexture信息确认
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
        /*controller.DisablePreview(); // 默认都不预览*/
    }

    public void DeleteCamera(CameraController controller)
    {
        if (cameraList.Contains(controller))
        {
            bool wasCurrent = (currentSelected == controller);

            if (wasCurrent)
            {
                currentSelected.DisablePreview();
                currentSelected = null;
            }

            cameraList.Remove(controller);

            //  删除整组
            if (controller.rigRoot != null)
            {
                Destroy(controller.rigRoot);
            }
            else
            {
                Destroy(controller.gameObject);
            }

            // 如果删的是当前选中，并且还有其他摄像机，自动切换到下一个
            if (wasCurrent && cameraList.Count > 0)
            {
                SelectCamera(cameraList[0]); // 你也可以选第一个：cameraList[0]
            }

            // 如果没摄像机了，就清空预览画面
            if (cameraList.Count == 0 && previewPlaneRenderer != null)
            {
                previewPlaneRenderer.material.mainTexture = null;
            }
        }
    }




    public void SelectCamera(CameraController controller)
    {
        // 1. 关闭之前的摄像机
        if (currentSelected != null && currentSelected != controller)
        {
            currentSelected.DisablePreview();
        }

        // 2. 设置新的摄像机为当前选择
        currentSelected = controller;

        // 3. 让它启用渲染到 RenderTexture
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

        //  告诉 Controller 它属于哪个 Rig（整体）
        controller.rigRoot = camObj;

        RegisterCamera(controller);
        SelectCamera(controller); // 添加完自动选中，预览该摄像机
    }

}
