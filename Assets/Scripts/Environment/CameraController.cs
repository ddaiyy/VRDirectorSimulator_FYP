using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static CameraController;
using MyGame.Selection;

public class CameraController : MonoBehaviour, ICustomSelectable
{
    public GameObject worldCanvas; // °´Å¥Canvas£¬¹ÒÔØÔÚÉãÏñ»úPrefabÏÂ
    public Camera mainPlayerCamera;
    public PostProcessVolume postProcessVolume;  // ÐÂÔö£ººóÆÚ´¦ÀíVolumeÒýÓÃ
    public DepthOfField dof;  // DepthOfFieldÉèÖÃ»º´æ
    private float focusDistance = 5f; // Ä¬ÈÏ5
    private Camera cam;
    [HideInInspector]
    public GameObject rigRoot; // Ìí¼Ó£ºÕû¸ö VirtualCameraRig µÄÒýÓÃ
    private void Awake()
    {
        if (mainPlayerCamera == null)
        {
            mainPlayerCamera = Camera.main;
        }

        cam = GetComponent<Camera>();

        // ÆôÓÃÉãÏñ»ú£¬µ«Ä¬ÈÏ²»Ð´Èë RT£¬·ÀÖ¹ºÚÆÁ bug
        cam.enabled = true;
        cam.targetTexture = null;

        if (postProcessVolume != null)
        {
            var oldProfile = postProcessVolume.profile;
            postProcessVolume.profile = Instantiate(oldProfile);
            Debug.Log($"Old Profile ID: {oldProfile.GetInstanceID()}, New Profile ID: {postProcessVolume.profile.GetInstanceID()}");

            if (!postProcessVolume.profile.TryGetSettings(out dof))
            {
                Debug.LogError("PostProcessProfile ÖÐÃ»ÓÐÕÒµ½ DepthOfField ÉèÖÃ£¡");
            }
        }


    }

    private void Start()
    {
        // Ö÷ÉãÏñ»ú±£³ÖÖ÷»­Ãæ
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
        Debug.Log($"启用预览 {gameObject.name}");
        cam.enabled = false;
        cam.targetTexture = null;  // 先清空一下，确保刷新
        cam.targetTexture = rt;

        // ·ÀÖ¹Êä³öµ½Ö÷ÆÁÄ»£ºÖ»ÓÐÉèÖÃ targetTexture ºóÔÙÆôÓÃÉãÏñ»ú
        if (rt != null)
        {
            cam.enabled = true;
        }

        /*if (worldCanvas != null)
            worldCanvas.SetActive(false);*/
    }


    public void DisablePreview()
    {
        cam.enabled = false;
        cam.targetTexture = null;
        Debug.Log($"DisablePreview called on {gameObject.name}");
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
            Debug.LogWarning("worldCanvas Î´¸³Öµ");
            return;
        }

        bool isActive = worldCanvas.activeSelf;
        worldCanvas.SetActive(!isActive);

        Debug.Log($"worldCanvas ×´Ì¬ÇÐ»»Îª {!isActive}");
    }*/
    public void SetFocusDistance(float value)
    {
        if (dof != null)
        {
            dof.focusDistance.value = value;
            dof.enabled.value = true;  // È·±£¾°Éî¿ªÆô
            Debug.Log($"ÉèÖÃ¾°Éî½¹¾àÎª {value}");
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

    public void OnSelect()
    {
        Debug.Log($"{gameObject.name} 被选中：OnSelect 被调用");

        worldCanvas?.SetActive(true);
        CameraManager.Instance.SelectCamera(this);
    }


    public void OnDeselect()
    {
        Debug.Log($"{gameObject.name} È¡ÏûÑ¡ÖÐ");
    }

    [ContextMenu("Test OnSelect")]
    private void TestOnSelect()
    {
        OnSelect();
    }

    [ContextMenu("Test OnDeselect")]
    private void TestOnDeselect()
    {
        OnDeselect();
    }

}
