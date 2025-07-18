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
            if (currentSelected == controller)
            {
                currentSelected.DisablePreview();
                currentSelected = null;
            }

            cameraList.Remove(controller);

            // 销毁整个虚拟摄像机结构（而不仅是摄像机组件）
            if (controller.rigRoot != null)
            {
                Destroy(controller.rigRoot);
            }
            else
            {
                // 回退：如果 rigRoot 没有设置，就删除自身
                Destroy(controller.gameObject);
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
