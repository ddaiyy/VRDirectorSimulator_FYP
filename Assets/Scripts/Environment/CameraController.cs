using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public GameObject worldCanvas; // ��ťCanvas�������������Prefab��
    public Camera mainPlayerCamera;
    public PostProcessVolume postProcessVolume;  // ���������ڴ���Volume����
    public DepthOfField dof;  // DepthOfField���û���
    private float focusDistance = 5f; // Ĭ��5
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

        if (postProcessVolume != null)
        {
            var oldProfile = postProcessVolume.profile;
            postProcessVolume.profile = Instantiate(oldProfile);
            Debug.Log($"Old Profile ID: {oldProfile.GetInstanceID()}, New Profile ID: {postProcessVolume.profile.GetInstanceID()}");

            if (!postProcessVolume.profile.TryGetSettings(out dof))
            {
                Debug.LogError("PostProcessProfile ��û���ҵ� DepthOfField ���ã�");
            }
        }


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
            Debug.LogWarning("worldCanvas δ��ֵ");
            return;
        }

        bool isActive = worldCanvas.activeSelf;
        worldCanvas.SetActive(!isActive);

        Debug.Log($"worldCanvas ״̬�л�Ϊ {!isActive}");
    }*/
    public void SetFocusDistance(float value)
    {
        if (dof != null)
        {
            dof.focusDistance.value = value;
            dof.enabled.value = true;  // ȷ�������
            Debug.Log($"���þ����Ϊ {value}");
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
