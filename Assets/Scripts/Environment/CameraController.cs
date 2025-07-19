using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public GameObject worldCanvas; // 按钮Canvas，挂载在摄像机Prefab下
    public Camera mainPlayerCamera;
    public PostProcessVolume postProcessVolume;  // 新增：后期处理Volume引用
    public DepthOfField dof;  // DepthOfField设置缓存
    private float focusDistance = 5f; // 默认5
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

        if (postProcessVolume != null)
        {
            var oldProfile = postProcessVolume.profile;
            postProcessVolume.profile = Instantiate(oldProfile);
            Debug.Log($"Old Profile ID: {oldProfile.GetInstanceID()}, New Profile ID: {postProcessVolume.profile.GetInstanceID()}");

            if (!postProcessVolume.profile.TryGetSettings(out dof))
            {
                Debug.LogError("PostProcessProfile 中没有找到 DepthOfField 设置！");
            }
        }


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

        /*if (worldCanvas != null)
            worldCanvas.SetActive(false);*/
    }


    public void DisablePreview()
    {
        cam.targetTexture = null;
        cam.enabled = false;

        /*if (worldCanvas != null)
            worldCanvas.SetActive(false);*/
    }


    public void SelectThisCamera()
    {
        CameraManager.Instance.SelectCamera(this);
    }

    public void DeleteThisCamera()
    {
        CameraManager.Instance.DeleteCamera(this);
    }

    public void SetFOV(float value)
    {
        cam.fieldOfView = value;
    }

    public float GetFOV()
    {
        return cam.fieldOfView;
    }

    /*[ContextMenu("Toggle WorldCanvas")]
    public void ToggleWorldCanvas()
    {
        if (worldCanvas == null)
        {
            Debug.LogWarning("worldCanvas 未赋值");
            return;
        }

        bool isActive = worldCanvas.activeSelf;
        worldCanvas.SetActive(!isActive);

        Debug.Log($"worldCanvas 状态切换为 {!isActive}");
    }*/
    public void SetFocusDistance(float value)
    {
        if (dof != null)
        {
            dof.focusDistance.value = value;
            dof.enabled.value = true;  // 确保景深开启
            Debug.Log($"设置景深焦距为 {value}");
        }
    }
    public float GetFocusDistance()
    {
        if (dof != null)
            return dof.focusDistance.value;
        else
            return 0f;
    }

    [ContextMenu("Test Set Focus Distance 3")]
    private void TestSetFocusDistance()
    {
        SetFocusDistance(3f);
    }
}
