using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject worldCanvas; // 按钮Canvas，挂载在摄像机Prefab下
    public Camera mainPlayerCamera;

    private Camera cam;
    [HideInInspector]
    public GameObject rigRoot; // 添加：整个 VirtualCameraRig 的引用
    private void Awake()
    {
        if (mainPlayerCamera == null)
        {
            mainPlayerCamera = Camera.main;
        }

        cam = GetComponent<Camera>();

        // 启用摄像机，但默认不写入 RT，防止黑屏 bug
        cam.enabled = true;
        cam.targetTexture = null;

        if (worldCanvas != null)
            worldCanvas.SetActive(false); // 默认隐藏按钮UI
    }

    private void Start()
    {
        // 主摄像机保持主画面
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

        // 防止输出到主屏幕：只有设置 targetTexture 后再启用摄像机
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
