using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectTimelineUI : MonoBehaviour
{
    [Header("播放控制")]
    public TextMeshProUGUI titleText;
    public Slider timeSlider;               // 播放指针
    public TextMeshProUGUI currentTimeText;
    public Button playButton, stopButton;
    public Button addKeyframeButton, deleteKeyframeButton, clearAllKeyframeButton;

    [Header("关键帧 UI")]
    public Transform keyframeListContent;   // Scroll View/Viewport/Content/Keyframe Content List
    public GameObject propertyTrackPrefab;  // 每个属性轨道的Prefab
    public GameObject keyframeItemPrefab;   // 每个关键帧点的Prefab

    [Header("相机控制 (可选)")]
    public Toggle camActiveToggle;
    public Slider fovSlider, focusDistanceSlider;
    public Text fovText, focusDistanceValueText;

    [Header("时间轴缩放/滚动")]
    public RectTransform content;           // ScrollView Content
    public RectTransform viewport;          // ScrollView Viewport
    public ScrollRect scrollRect;           // ScrollView
    public Scrollbar timelineScrollbar;     // Scrollbar Horizontal
    public RectTransform sliderHandle;      // 缩放手柄（可选）
    public Button zoomInButton, zoomOutButton;

    [Header("时间参数")]
    public float totalTime = 300f;          // 时间轴总时长
    public float defaultVisibleTime = 20f;  // 默认显示范围
    public float zoomStep = 1.5f;           // 缩放步长
    public float minVisibleTime = 5f;
    public float maxVisibleTime = 120f;
    public float pixelsPerSecond = 20f;     // 每秒多少像素

    private float currentVisibleTime;
    [SerializeField] private TimelineTrack currentTrack;

    void Start()
    {
        currentVisibleTime = defaultVisibleTime;
        UpdateTimeline();

        // 绑定按钮
        playButton.onClick.AddListener(OnPlayClicked);
        stopButton.onClick.AddListener(OnStopClicked);
        addKeyframeButton.onClick.AddListener(OnAddKeyframeClicked);
        deleteKeyframeButton.onClick.AddListener(OnDeleteKeyframeClicked);
        clearAllKeyframeButton.onClick.AddListener(OnClearAllKeyframeClicked);

        timeSlider.onValueChanged.AddListener(OnSliderChanged);

        if (zoomInButton != null) zoomInButton.onClick.AddListener(ZoomIn);
        if (zoomOutButton != null) zoomOutButton.onClick.AddListener(ZoomOut);
    }

    void Update()
    {
        if (currentTrack != null && gameObject.activeSelf)
        {
            RefreshTime();
        }
    }

    // ================== 时间轴缩放/滚动 ==================
    public void ZoomIn()
    {
        currentVisibleTime = Mathf.Max(currentVisibleTime / zoomStep, minVisibleTime);
        UpdateTimeline();
        RefreshKeyframeList();
    }

    public void ZoomOut()
    {
        currentVisibleTime = Mathf.Min(currentVisibleTime * zoomStep, maxVisibleTime);
        UpdateTimeline();
        RefreshKeyframeList();
    }

    private void UpdateTimeline()
    {
        // 动态调整 Content 宽度
        float contentWidth = totalTime * pixelsPerSecond;
        content.sizeDelta = new Vector2(contentWidth, content.sizeDelta.y);

        // Scrollbar handle 宽度（代表可见范围比例）
        if (timelineScrollbar != null && sliderHandle != null)
        {
            RectTransform scrollbarRect = timelineScrollbar.GetComponent<RectTransform>();
            float scrollbarWidth = scrollbarRect.sizeDelta.x;
            float handleWidth = defaultVisibleTime / currentVisibleTime * scrollbarWidth;
            sliderHandle.sizeDelta = new Vector2(handleWidth, sliderHandle.sizeDelta.y);
            sliderHandle.anchoredPosition = new Vector2(handleWidth / 2f, sliderHandle.anchoredPosition.y);
        }
    }
    /*private void UpdateTimeline()
    {
        // 总时长像素
        float totalPixelLength = totalTime * pixelsPerSecond;

        // ✅ 保证 Content 宽度 = 时间长度 + Viewport 宽度
        float contentWidth = totalPixelLength + viewport.rect.width;
        content.sizeDelta = new Vector2(contentWidth, content.sizeDelta.y);

        if (timelineScrollbar != null && sliderHandle != null)
        {
            RectTransform scrollbarRect = timelineScrollbar.GetComponent<RectTransform>();
            float scrollbarWidth = scrollbarRect.sizeDelta.x;

            // handle 宽度 = 可见范围 / 总时间
            float handleWidth = Mathf.Max((currentVisibleTime / totalTime) * scrollbarWidth, 20f);
            sliderHandle.sizeDelta = new Vector2(handleWidth, sliderHandle.sizeDelta.y);
        }
    }*/

    // ================== 关键帧刷新 ==================
    void RefreshKeyframeList()
    {
        foreach (Transform child in keyframeListContent)
            Destroy(child.gameObject);

        string[] properties = { "Position", "Rotation", "Scale" };
        if (currentTrack.GetComponentInChildren<Camera>() != null)
            properties = new string[] { "Position", "Rotation", "Scale", "FOV", "DOF" };

        foreach (string prop in properties)
        {
            GameObject trackRow = Instantiate(propertyTrackPrefab, keyframeListContent);
            trackRow.GetComponentInChildren<TextMeshProUGUI>().text = prop;
            RectTransform trackContent = trackRow.transform.Find("Track Content").GetComponent<RectTransform>();

            foreach (var clip in currentTrack.clips)
            {
                GameObject point = Instantiate(keyframeItemPrefab, trackContent);
                float x = clip.time * pixelsPerSecond; // ✅ 关键帧位置按缩放计算
                point.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0);

                var text = point.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = $"{clip.time:F2}";
            }
        }
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    // ================== 播放控制 ==================
    void OnPlayClicked()
    {
        if (TimelineManager.Instance.isMasterControl) return;
        currentTrack?.Play();
    }

    void OnStopClicked()
    {
        if (TimelineManager.Instance.isMasterControl) return;
        currentTrack?.Stop();
    }

    void OnSliderChanged(float value)
    {
        if (TimelineManager.Instance.isMasterControl) return;
        if (currentTrack == null) return;

        currentTrack.SetSuppressAnimationOnReset(true);
        currentTrack.SetTime(value);
        currentTrack.SetSuppressAnimationOnReset(false);

        if (currentTrack.isCamera) RefreshCamera();
    }

    void OnAddKeyframeClicked()
    {
        if (currentTrack != null)
        {
            float time = timeSlider.value;
            currentTrack.AddClip(time);
            RefreshKeyframeList();
        }
    }

    void OnDeleteKeyframeClicked()
    {
        currentTrack?.DeleteClipAtTime(timeSlider.value);
    }

    void OnClearAllKeyframeClicked()
    {
        currentTrack?.DeleteAllClips();
    }

    // ================== 刷新 ==================
    void RefreshTime()
    {
        currentTimeText.text = $"Time: {currentTrack.currentTime:F2}";
        timeSlider.value = currentTrack.currentTime;
    }

    private void RefreshCamera()
    {
        if (camActiveToggle == null || currentTrack == null) return;
        camActiveToggle.isOn = currentTrack.GetPreCameraClipActive(currentTrack.currentTime);
        fovSlider.value = currentTrack.cameraController.GetFOV();
        fovText.text = fovSlider.value.ToString("F0");
        focusDistanceSlider.value = currentTrack.cameraController.GetFocusDistance();
        focusDistanceValueText.text = focusDistanceSlider.value.ToString("F0");
    }

    public void Initialize(TimelineTrack track)
    {
        currentTrack = track;
        gameObject.SetActive(true);
        titleText.text = "Timeline- " + track.gameObject.name;

        timeSlider.minValue = 0;
        timeSlider.maxValue = totalTime;
        timeSlider.value = track.currentTime;

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (currentTrack == null) return;
        RefreshKeyframeList();
        RefreshTime();
        if (currentTrack.isCamera) RefreshCamera();
    }
}
