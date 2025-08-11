using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectTimelineUI : MonoBehaviour
{
    [Header("UI引用")]
    //public Text titleText;
    public TextMeshProUGUI titleText;

    public Button playButton;
    public Button stopButton;

    public Slider timeSlider;

    //public Text currentTimeText;
    public TextMeshProUGUI currentTimeText;

    public Transform keyframeListContent;

    //public Transform propertyTrackListContent; // Vertical Layout Group
    public GameObject propertyTrackPrefab; // 每个属性轨道的Prefab

    public GameObject keyframeItemPrefab;
    public Button addKeyframeButton;

    public RectTransform timelineContent; // 拖到Inspector
    public float timelineWidth = 600f; // 你的时间轴UI宽度
    public float maxTime = 20f; // 时间轴最大时间

    public Button deleteKeyframeButton;
    public Button clearAllKeyframeButton;
    
    [Header("Camera相关")]
    public Toggle camActiveToggle;
    public Slider fovSlider;
    public Text fovText;
    public Slider focusDistanceSlider;
    public Text focusDistanceValueText;
    
    
    [Header("对应TimelineObject")]
    [SerializeField] private TimelineTrack currentTrack;

    void Start()
    {
        // 绑定按钮事件
        playButton.onClick.AddListener(OnPlayClicked);
        stopButton.onClick.AddListener(OnStopClicked);
        timeSlider.onValueChanged.AddListener(OnSliderChanged);
        addKeyframeButton.onClick.AddListener(OnAddKeyframeClicked);
        deleteKeyframeButton.onClick.AddListener(OnDeleteKeyframeClicked);
        clearAllKeyframeButton.onClick.AddListener(OnClearAllKeyframeClicked);
        
        //SelectCamera
        if (camActiveToggle != null)
        {
            camActiveToggle.onValueChanged.AddListener(OnCamActiveChanged);
            //dof
            focusDistanceSlider.minValue = 0.1f;
            focusDistanceSlider.maxValue = 10f;
            focusDistanceSlider.onValueChanged.AddListener(OnFocusDistanceChanged);
            //fov
            fovSlider.onValueChanged.AddListener(OnFOVChanged);
            
            
        }
        //HidePanel();
    }

    private void OnClearAllKeyframeClicked()
    {
        currentTrack.DeleteAllClips();
    }

    private void OnDeleteKeyframeClicked()
    {
        currentTrack.DeleteClipAtTime(timeSlider.value);
    }

    private void OnFOVChanged(float value)
    {
        currentTrack.cameraController.SetFOV(value);
        fovText.text = value.ToString("F0");
    }

    private void OnFocusDistanceChanged(float value)
    {
        currentTrack.cameraController.SetFocusDistance(value);
        focusDistanceValueText.text = value.ToString("F0");
    }

    // 处理相机激活状态变化
    private void OnCamActiveChanged(bool isActive)
    {
        if (currentTrack != null && currentTrack.isCamera)
        {
            // 更新当前关键帧的激活状态
            var currentClip = currentTrack.GetClipAtTime(currentTrack.currentTime);
            if (currentClip != null)
            {
                currentClip.isCameraActiveAtTime= isActive;
                Debug.Log($"[{currentTrack.gameObject.name}] 相机激活状态设置为: {isActive}");
            }
            
            // 如果当前轨道对应的相机是当前选中的相机，更新CameraManager
            if (CameraManager.Instance != null)
            {
                var currentSelectedCamera = CameraManager.Instance.GetCurrentSelectedCameraController();
                if (currentSelectedCamera != null && currentSelectedCamera!=currentTrack.cameraController)
                {
                    if (isActive)
                    {
                        CameraManager.Instance.SelectCamera(currentSelectedCamera);
                    }
                    else
                    {
                        CameraManager.Instance.ClearSelectedCamera(currentSelectedCamera);
                    }
                }
            }
        }
    }
    
    public void ShowPanel(TimelineTrack track)
    {
        Initialize(track);
        RefreshAll();
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentTrack != null && gameObject.activeSelf)
        {
            RefreshTime();
        }
    }

    private void RefreshCamera()
    {
        camActiveToggle.isOn = currentTrack.GetPreCameraClipActive(currentTrack.currentTime);
        fovSlider.value = currentTrack.cameraController.GetFOV();
        fovText.text=fovSlider.value.ToString("F0");

        focusDistanceSlider.value = currentTrack.cameraController.GetFocusDistance();
        focusDistanceValueText.text = focusDistanceSlider.value.ToString("F0");
    }

    void RefreshTime()
    {
        currentTimeText.text = $"Time: {currentTrack.currentTime:F2}";
        timeSlider.value = currentTrack.currentTime;
    }

    void RefreshKeyframeList()
    {
        // 清空旧的
        foreach (Transform child in keyframeListContent)
            Destroy(child.gameObject);

        // 你要支持的属性
        string[] properties = { "Position", "Rotation", "Scale" };
        // 如果是摄像机，加上FOV
        if (currentTrack.GetComponentInChildren<Camera>() != null)
        {
            properties = new string[] { "Position", "Rotation", "Scale", "FOV" };
        }

        foreach (string prop in properties)
        {
            GameObject trackRow = Instantiate(propertyTrackPrefab, keyframeListContent);
            trackRow.GetComponentInChildren<TextMeshProUGUI>().text = prop;

            RectTransform trackContent = trackRow.transform.Find("Track Content").GetComponent<RectTransform>();

            // 这里直接遍历所有关键帧
            foreach (var clip in currentTrack.clips)
            {
                GameObject point = Instantiate(keyframeItemPrefab, trackContent);
                float normalizedTime = Mathf.Clamp01(clip.time / maxTime);
                float x = normalizedTime * timelineWidth;
                var rect = point.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(x, 0);

                // 可选：显示时间
                var text = point.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = $"{clip.time:F2}";

                // 可选：点击跳转
                var btn = point.GetComponent<Button>();
                if (btn != null)
                {
                    float t = clip.time;
                    btn.onClick.AddListener(() =>
                    {
                        // 如果主时间轴正在控制，禁用关键帧跳转
                        if(TimelineManager.Instance.isMasterControl) return;
                        
                        // 设置抑制动画标志，防止在跳转时意外触发动画
                        currentTrack.SetSuppressAnimationOnReset(true);
                        currentTrack.SetTime(t);
                        currentTrack.SetSuppressAnimationOnReset(false);
                        
                        RefreshTime();
                        if (currentTrack.isCamera)
                        {
                             RefreshCamera();
                        }
                    });
                }
            }
        }
    }

    void OnPlayClicked()
    {
        // 如果主时间轴正在控制，禁用单个轨道的播放
        if(TimelineManager.Instance.isMasterControl) return;
        
        if (currentTrack != null)
            currentTrack.Play();
    }

    void OnStopClicked()
    {
        // 如果主时间轴正在控制，禁用单个轨道的停止
        if(TimelineManager.Instance.isMasterControl) return;
        
        if (currentTrack != null)
            currentTrack.Stop();
    }

    void OnSliderChanged(float value)
    {
        // 如果主时间轴正在控制，禁用UI时间滑块操作
        if(TimelineManager.Instance.isMasterControl) return;
        
        if (currentTrack != null)
        {
            // 设置抑制动画标志，防止在手动拖动时意外触发动画
            currentTrack.SetSuppressAnimationOnReset(true);
            currentTrack.SetTime(value);
            currentTrack.SetSuppressAnimationOnReset(false);

            if (currentTrack.isCamera)
            {
                RefreshCamera();
            }
        }
    }

    void OnAddKeyframeClicked()
    {
        if (currentTrack != null)
        {
            float time = timeSlider.value;
            AddKeyframeAtTime(currentTrack, time);
            RefreshKeyframeList();
        }
    }

    void AddKeyframeAtTime(TimelineTrack track, float time)
    {
        track.AddClip(time);
    }

    public void Initialize(TimelineTrack track)
    {
        currentTrack = track;
        gameObject.SetActive(true);

        titleText.text = "Timeline- " + track.gameObject.name;
        timeSlider.minValue = 0;
        timeSlider.maxValue = 20f;
        timeSlider.value = track.currentTime;

        // 如果是相机轨道，同步激活状态
        if (track.isCamera)
        {
            var currentClip = track.GetClipAtTime(track.currentTime);
            if (currentClip != null)
            {
                camActiveToggle.isOn = currentClip.isCameraActiveAtTime;
                focusDistanceSlider.value = currentClip.focusDistance;
                fovSlider.value = currentClip.fov;
                
                focusDistanceValueText.text = currentClip.focusDistance.ToString("F0");
                fovText.text=currentClip.fov.ToString("F0");
            }
            else
            {
                // 如果没有关键帧，默认激活
                camActiveToggle.isOn = false;
                focusDistanceSlider.value = track.startFocusDistance;
                fovSlider.value = track.startFov;
                
                fovText.text = track.startFov.ToString("F0");
                focusDistanceValueText.text = track.startFocusDistance.ToString("F0");
            }
        }

    }

    public void RefreshAll()
    {
        if (currentTrack == null) return;

        Debug.Log("[ObjectTimelineUI] 刷新所有UI内容");
        
        // 更新时间轴范围
        timeSlider.minValue = 0;
        timeSlider.maxValue = 20f;//TODO:可能需要更改
        timeSlider.value = currentTrack.currentTime;

        // 刷新关键帧列表
        RefreshKeyframeList();

        // 刷新时间显示
        RefreshTime();

        // 如果是相机轨道，同步激活状态
        if (currentTrack.isCamera )
        {
            RefreshCamera();
        }
    }
    
}