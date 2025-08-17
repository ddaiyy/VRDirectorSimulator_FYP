using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectTimelineUI : MonoBehaviour
{
    [Header("标题/播放控制")]
    public TextMeshProUGUI titleText;
    public Slider timeSlider;               // 放在 Content 下：轨道长度 = Content 宽；范围恒 0..totalTime
    public TextMeshProUGUI currentTimeText;
    public Button playButton, stopButton;
    public Button addKeyframeButton, deleteKeyframeButton, clearAllKeyframeButton;

    [Header("关键帧 UI（每个 Track 一行）")]
    public Transform keyframeListContent;   // Scroll View/Viewport/Content/Keyframe Rows 容器
    public GameObject propertyTrackPrefab;  // 需含子节点 "Track Content"（作为本行的点的容器）
    public GameObject keyframeItemPrefab;   // 关键帧点 Prefab（Button + Text 可选）
    
    
    [Header("相机控制(可选)")]
    public Toggle camActiveToggle;
    public Slider fovSlider, focusDistanceSlider;
    public Text fovText, focusDistanceValueText;

    [Header("滚动/缩放（ScrollView 独立控制）")]
    public RectTransform viewport;          // Viewport（裁剪窗口）
    public RectTransform content;           // Content（不要 Layout/Fitter，锚点左上）
    public ScrollRect scrollRect;           // 仅裁剪，不绑定水平滚动条
    public Scrollbar timelineScrollbar;     // 独立水平滚动条
    public Button zoomInButton, zoomOutButton;

    [Header("时间参数")]
    public float totalTime = 60f;           // 总时长（秒）
    public float defaultVisibleTime = 20f;  // 每页可见（秒）
    public float zoomStep = 1.5f;           // 缩放因子
    public float minVisibleTime = 5f;       // 硬下限（安全）
    public float maxVisibleTime = 120f;     // 硬上限（安全）

    [Header("缩放阈值（可自定义）")]
    [Tooltip("当每页可见时间 <= minPageSeconds 时，禁止继续放大（即再减小可见时间）。例如 10s。")]
    public float minPageSeconds = 10f;
    [Tooltip("当每页可见时间 >= maxPageSeconds 时，禁止继续缩小（即再增大可见时间）。例如 30s。")]
    public float maxPageSeconds = 30f;

    [Header("几何与缓冲")]
    public float rightEdgePaddingPx = 24f;  // Content 右侧缓冲像素，避免滚到尽头顶死
    public float topRowGapPx = 0f;          // 行整体向下偏移（上缘坐标系，向下为负）

    [Header("安全选项")]
    public bool autoFixConflictingLayoutOnContent = true;

    
    [Header("背景条（覆盖关键帧区域，不随缩放变高）")]
    public RectTransform timelineBackground;        // 拖 Content 下的 Image/RawImage
    public float timelineBackgroundPaddingTop = 4f; // 上内边距
    public float timelineBackgroundPaddingBottom = 4f; // 下内边距

    [Header("布局偏移")]
    [Tooltip("左侧轨道名列的固定像素宽度；0s 从该列右边开始")]
    public float nameColumnWidthPx = 120f;

    // 运行态
    private float currentVisibleTime;
    private float pixelsPerSecond;          // pps = viewport.width / currentVisibleTime
    [SerializeField] private TimelineTrack currentTrack;

    private float _lastSetContentWidth = -1f;
    private bool _programmaticScroll = false;

    // 便捷：有效缩放边界（把安全上下限与业务阈值合并）
    private float EffectiveMinPage => Mathf.Max(minVisibleTime, minPageSeconds);
    private float EffectiveMaxPage => Mathf.Min(maxVisibleTime, maxPageSeconds);

    // ---------------- 生命周期 ----------------
    private void Awake()
    {
        // Content 左上对齐（上缘坐标系，便于纵向布局）
        if (content != null)
        {
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(0f, 1f);
            content.pivot     = new Vector2(0f, 1f);
        }

        // 可选：移除会改宽度的布局组件
        if (autoFixConflictingLayoutOnContent && content != null)
        {
            var fitter = content.GetComponent<ContentSizeFitter>(); if (fitter) fitter.enabled = false;
            var lg     = content.GetComponent<LayoutGroup>();       if (lg) lg.enabled = false;
            var le     = content.GetComponent<LayoutElement>();     if (le) le.ignoreLayout = true;
        }
    }

    private void Start()
    {
        // ScrollRect 只裁剪
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
            scrollRect.inertia = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.elasticity = 0f;
            scrollRect.horizontalScrollbar = null; // 不绑定
        }

        if (timelineScrollbar != null)
        {
            timelineScrollbar.numberOfSteps = 0;
            timelineScrollbar.onValueChanged.AddListener(OnTimelineScroll);
        }

        // ✅ 默认页宽也受可自定义阈值夹取
        currentVisibleTime = Mathf.Clamp(defaultVisibleTime, EffectiveMinPage, EffectiveMaxPage);

        // 交互
        if (playButton) playButton.onClick.AddListener(OnPlayClicked);
        if (stopButton)  stopButton.onClick.AddListener(OnStopClicked);
        if (addKeyframeButton) addKeyframeButton.onClick.AddListener(OnAddKeyframeClicked);
        if (deleteKeyframeButton) deleteKeyframeButton.onClick.AddListener(OnDeleteKeyframeClicked);
        if (clearAllKeyframeButton) clearAllKeyframeButton.onClick.AddListener(OnClearAllKeyframeClicked);
        if (zoomInButton)  zoomInButton.onClick.AddListener(ZoomIn);
        if (zoomOutButton) zoomOutButton.onClick.AddListener(ZoomOut);

        if (timeSlider)
        {
            timeSlider.wholeNumbers = false;
            timeSlider.minValue = 0f;
            timeSlider.maxValue = totalTime;    // 全局范围
            timeSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        if (camActiveToggle) camActiveToggle.onValueChanged.AddListener(OnCamActiveChanged);
        if (fovSlider) fovSlider.onValueChanged.AddListener(OnFOVChanged);
        if (focusDistanceSlider)
        {
            focusDistanceSlider.minValue = 0.1f;
            focusDistanceSlider.maxValue = 10f;
            focusDistanceSlider.onValueChanged.AddListener(OnFocusDistanceChanged);
        }

        UpdateTimeline();   // 计算 pps，设置 Content/Slider/背景 宽度，同步滚动条
        RefreshAll();       // 生成行与关键帧
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
        UpdateZoomButtons(); // ✅ 初始化按钮状态
    }

    private void OnDestroy()
    {
        if (timelineScrollbar) timelineScrollbar.onValueChanged.RemoveListener(OnTimelineScroll);
        if (timeSlider) timeSlider.onValueChanged.RemoveListener(OnSliderChanged);
        if (camActiveToggle) camActiveToggle.onValueChanged.RemoveListener(OnCamActiveChanged);
        if (fovSlider) fovSlider.onValueChanged.RemoveListener(OnFOVChanged);
        if (focusDistanceSlider) focusDistanceSlider.onValueChanged.RemoveListener(OnFocusDistanceChanged);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled) return;
        UpdateTimeline();
        RefreshKeyframeList(); // 重新定位关键帧点的 x
        UpdateZoomButtons();
    }

    private void Update()
    {
        if (currentTrack != null && gameObject.activeSelf)
            RefreshTime();

        // 守护：外部改了 content 宽就回写
        if (_lastSetContentWidth >= 0f && content != null)
        {
            float now = content.sizeDelta.x;
            if (!Mathf.Approximately(now, _lastSetContentWidth))
            {
                content.sizeDelta = new Vector2(_lastSetContentWidth, content.sizeDelta.y);
                Debug.LogWarning($"[Timeline] 检测到外部修改 Content 宽度({now:F1})，已回写为 {_lastSetContentWidth:F1}");
            }
        }
    }

    // ---------------- 缩放/滚动 ----------------
    public void ZoomIn()
    {
        // 放大 = 每页显示更少秒
        float target = currentVisibleTime / zoomStep;
        // 受可自定义阈值限制
        target = Mathf.Max(target, EffectiveMinPage);

        if (NearlyEqual(target, currentVisibleTime)) return;

        currentVisibleTime = target;
        ClampZoom();             // 与硬上下限再对齐一次
        UpdateTimeline();
        RefreshKeyframeList();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
        UpdateZoomButtons();     // ✅ 按钮交互
    }

    public void ZoomOut()
    {
        // 缩小 = 每页显示更多秒
        float target = currentVisibleTime * zoomStep;
        // 受可自定义阈值限制
        target = Mathf.Min(target, EffectiveMaxPage);

        if (NearlyEqual(target, currentVisibleTime)) return;

        currentVisibleTime = target;
        ClampZoom();
        UpdateTimeline();
        RefreshKeyframeList();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
        UpdateZoomButtons();     // ✅ 按钮交互
    }

    // 保证缩放范围在 minVisibleTime ~ maxVisibleTime 之间（安全护栏）
    private void ClampZoom()
    {
        float viewportWidth = viewport ? viewport.rect.width : 0f;
        if (viewportWidth <= 0f) return;

        currentVisibleTime = Mathf.Clamp(currentVisibleTime, Mathf.Max(minVisibleTime, 0.0001f), maxVisibleTime);
        // 像素/秒由 UpdateTimeline 统一计算，这里不直接改 pps
    }

    private void UpdateZoomButtons()
    {
        if (zoomInButton)  zoomInButton.interactable  = currentVisibleTime > EffectiveMinPage + 0.001f;
        if (zoomOutButton) zoomOutButton.interactable = currentVisibleTime < EffectiveMaxPage - 0.001f;
    }

    private static bool NearlyEqual(float a, float b, float eps = 0.0001f) => Mathf.Abs(a - b) < eps;

    private void UpdateTimeline()
{
    Canvas.ForceUpdateCanvases();
    float viewportWidth = viewport ? viewport.rect.width : 0f;
    if (viewportWidth <= 0f) return;

    // 一页（viewport）= currentVisibleTime 秒
    pixelsPerSecond = viewportWidth / Mathf.Max(currentVisibleTime, 0.0001f);

    // 时间区域宽度（不含左列）
    float timelineWidth = totalTime * pixelsPerSecond;

    // Content 总宽 = 左列 + 时间区域 + 右缓冲
    float contentWidth = nameColumnWidthPx + timelineWidth + Mathf.Max(rightEdgePaddingPx, 0f);
    SetContentWidth(contentWidth);

    // ---------------- 背景条覆盖时间区域（并保证高度为正） ----------------
    if (timelineBackground)
    {
        var bg = timelineBackground;

        // 与 Content 一致的锚点/枢轴：左上
        bg.anchorMin = new Vector2(0f, 1f);
        bg.anchorMax = new Vector2(0f, 1f);
        bg.pivot     = new Vector2(0f, 1f);

        // 宽：只覆盖“时间区域”（不含左侧名字列）
        float bgWidth = Mathf.Max(1f, timelineWidth);

        // 高：使用 Content 的真实高度（先强制重建一次布局，拿到最新 rect）
        var contentRT = content as RectTransform;
        if (contentRT)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);

        float contentHeight = contentRT ? Mathf.Max(1f, contentRT.rect.height) : 100f; // 兜底 100

        // 应用尺寸（高度=整个 Content 高度；宽度=时间区域宽度）
        bg.sizeDelta = new Vector2(bgWidth, contentHeight);

        // 位置：X 从“名字列右边”开始，Y 贴住 Content 顶部（上锚坐标系）
        bg.anchoredPosition = new Vector2(nameColumnWidthPx, 0f);

        // 放到底层，避免遮挡交互
        bg.SetAsFirstSibling();

        // 可见性兜底
        var img = bg.GetComponent<Image>();
        if (img)
        {
            if (img.sprite == null && img.color.a <= 0.01f) img.color = new Color(0,0,0,0.12f);
            img.raycastTarget = false;
        }
        var raw = bg.GetComponent<RawImage>();
        if (raw)
        {
            if (raw.texture == null && raw.color.a <= 0.01f) raw.color = new Color(0,0,0,0.12f);
            raw.raycastTarget = false;
        }

        // 防止被意外翻转（高度绝对为正）
        var ls = bg.localScale;
        bg.localScale = new Vector3(Mathf.Abs(ls.x) < 1e-3f ? 1f : Mathf.Abs(ls.x),
            Mathf.Abs(ls.y) < 1e-3f ? 1f : Mathf.Abs(ls.y),
            1f);
    }

    // ----------------------------------------------------------------------

    // timeSlider 轨道只覆盖“时间区域”，并从 0s 位置开始
    if (timeSlider)
    {
        var rt = timeSlider.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(timelineWidth, rt.sizeDelta.y);
        rt.anchoredPosition = new Vector2(nameColumnWidthPx, -topRowGapPx);
        timeSlider.minValue = 0f;
        timeSlider.maxValue = totalTime;
    }

    // 滚动条把手比例 = 可见/总
    if (timelineScrollbar)
    {
        timelineScrollbar.size = Mathf.Clamp01(currentVisibleTime / Mathf.Max(totalTime, 0.0001f));
        float scrollRangePx = Mathf.Max(contentWidth - viewportWidth, 0f);
        if (scrollRangePx < 1f)
        {
            _programmaticScroll = true;
            timelineScrollbar.value = 0f;
            _programmaticScroll = false;
        }
    }

    // 应用当前位置
    OnTimelineScroll(timelineScrollbar ? timelineScrollbar.value : 0f);
}



    private void SetContentWidth(float w)
    {
        if (!content) return;
        content.sizeDelta = new Vector2(w, content.sizeDelta.y);
        _lastSetContentWidth = w;
    }

    /// 滚动条 value → Content 偏移（含把手补偿 + 末端吸附）
    private void OnTimelineScroll(float normalized)
    {
        if (_programmaticScroll) return;

        float viewportWidth = viewport.rect.width;
        float contentWidth  = content.sizeDelta.x;
        float scrollRangePx = Mathf.Max(contentWidth - viewportWidth, 0f);

        if (scrollRangePx < 1f)
        {
            content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);
            return;
        }

        float size  = timelineScrollbar ? Mathf.Clamp01(timelineScrollbar.size) : 0f;
        float range = Mathf.Max(1f - size, 0.0001f);
        float nComp = Mathf.Clamp01(normalized / range);
        if (nComp > 0.98f) nComp = 1f;

        float offsetX = nComp * scrollRangePx;
        content.anchoredPosition = new Vector2(-offsetX, content.anchoredPosition.y);
    }

    /// 保证时间 t 在视窗内（必要时滚动视图）
    /// 保证时间 t 在视窗内（必要时滚动视图）——考虑左侧名字列宽
    private void EnsureTimeVisible(float t)
    {
        if (!timelineScrollbar) return;

        float viewportWidth = viewport.rect.width;
        float contentWidth  = content.sizeDelta.x;
        float pps           = Mathf.Max(pixelsPerSecond, 0.0001f);

        // 当前视窗的时间范围（左缘要扣除名字列）
        float viewStartSec = GetViewStartSec();
        float viewEndSec   = viewStartSec + currentVisibleTime;
        if (t >= viewStartSec && t <= viewEndSec) return;

        // 目标左缘时间（尽量把 t 放屏幕中间，再做边界夹取）
        float maxStart = Mathf.Max(totalTime - currentVisibleTime, 0f);
        float targetStart = Mathf.Clamp(t - currentVisibleTime * 0.5f, 0f, maxStart);

        // ← 关键：把目标时间左缘转成 Content 坐标的像素时，加回左列宽
        float desiredOffsetX = nameColumnWidthPx + targetStart * pps;

        float scrollRangePx  = Mathf.Max(contentWidth - viewportWidth, 0f);
        desiredOffsetX       = Mathf.Clamp(desiredOffsetX, 0f, scrollRangePx);

        // 反推 scrollbar.value（考虑把手尺寸）
        float size  = Mathf.Clamp01(timelineScrollbar.size);
        float range = Mathf.Max(1f - size, 0.0001f);
        float nComp = (scrollRangePx <= 0f) ? 0f : desiredOffsetX / scrollRangePx;
        float value = Mathf.Clamp01(nComp * range);

        _programmaticScroll = true;
        timelineScrollbar.value = value; // 触发 OnTimelineScroll → 真正移动 content
        _programmaticScroll = false;
    }


    
    // 视窗左缘对应的“时间秒”（考虑左侧名字列的像素偏移）
    private float GetViewStartSec()
    {
        float pps = Mathf.Max(pixelsPerSecond, 0.0001f);
        float leftX = -content.anchoredPosition.x; // 视窗左缘在 Content 坐标里的 X
        float t = (leftX - nameColumnWidthPx) / pps; // 扣除左列宽，再换算成秒
        return Mathf.Max(t, 0f);
    }
    
    // ---------------- 通用：将时间点安全放入任意 Track 行 ----------------
    private void PlacePointAtTime(RectTransform point, RectTransform trackContent, float t)
    {
        float xInContent = nameColumnWidthPx + t * Mathf.Max(pixelsPerSecond, 0.0001f);
        Vector3 world    = content.TransformPoint(new Vector3(xInContent, 0f, 0f)); // Content→世界
        Vector3 local    = trackContent.InverseTransformPoint(world);               // 世界→Track局部
        point.anchoredPosition = new Vector2(local.x, 0f);
    }


    // ---------------- 关键帧渲染/对齐 ----------------
    private float TimeToX(float t) => t * Mathf.Max(pixelsPerSecond, 0.0001f); // 仍可用在自定义绘制里

    private void RefreshKeyframeList()
    {
        foreach (Transform child in keyframeListContent)
            Destroy(child.gameObject);

        string[] properties = { "Position", "Rotation", "Scale" };
        if (currentTrack != null && currentTrack.GetComponentInChildren<Camera>() != null)
            properties = new[] { "Position", "Rotation", "Scale", "FOV", "DOF" };

        float rowYOffset = -topRowGapPx; // Content 以上缘为 0，向下为负
        foreach (string prop in properties)
        {
            GameObject row = Instantiate(propertyTrackPrefab, keyframeListContent);
            row.GetComponentInChildren<TextMeshProUGUI>().text = prop;

            RectTransform rowRT = row.GetComponent<RectTransform>();
            if (rowRT) rowRT.anchoredPosition = new Vector2(rowRT.anchoredPosition.x, rowYOffset);

            RectTransform trackContent = row.transform.Find("Track Content") as RectTransform;
            if (trackContent == null) continue;

            if (currentTrack != null)
            {
                foreach (var clip in currentTrack.clips)
                {
                    GameObject point = Instantiate(keyframeItemPrefab, trackContent);
                    RectTransform rect = point.GetComponent<RectTransform>();

                    // 坐标转换，保证与 Handle 垂直对齐
                    PlacePointAtTime(rect, trackContent, clip.time);

                    var text = point.GetComponentInChildren<TextMeshProUGUI>();
                    if (text) text.text = $"{clip.time:F2}";

                    var btn = point.GetComponent<Button>();
                    if (btn != null)
                    {
                        float t = clip.time;
                        btn.onClick.AddListener(() =>
                        {
                            currentTrack.SetSuppressAnimationOnReset(true);
                            currentTrack.SetTime(t);
                            currentTrack.SetSuppressAnimationOnReset(false);

                            if (timeSlider) timeSlider.value = t;
                            RefreshTime();
                            if (currentTrack.isCamera) RefreshCamera();
                            EnsureTimeVisible(t);
                        });
                    }
                }
            }
        }
    }

    // ---------------- 播放控制 ----------------
    private void OnPlayClicked()  { currentTrack?.Play(); }
    private void OnStopClicked()  { currentTrack?.Stop(); }
    private void OnAddKeyframeClicked()
    {
        if (currentTrack == null || timeSlider == null) return;
        float t = Mathf.Clamp(timeSlider.value, 0f, totalTime);
        currentTrack.AddClip(t);
        RefreshKeyframeList();
    }
    private void OnDeleteKeyframeClicked()
    {
        if (currentTrack == null || timeSlider == null) return;
        currentTrack.DeleteClipAtTime(Mathf.Clamp(timeSlider.value, 0f, totalTime));
        RefreshKeyframeList();
    }
    private void OnClearAllKeyframeClicked()
    {
        currentTrack?.DeleteAllClips();
        RefreshKeyframeList();
    }

    // 用户拖动时间条 → 设置时间并保证可见
    private void OnSliderChanged(float value)
    {
        if (currentTrack == null) return;
        float t = Mathf.Clamp(value, 0f, totalTime);

        currentTrack.SetSuppressAnimationOnReset(true);
        currentTrack.SetTime(t);
        currentTrack.SetSuppressAnimationOnReset(false);

        EnsureTimeVisible(t);
        if (currentTrack.isCamera) RefreshCamera();
    }

    // ---------------- 刷新（含相机） ----------------
    private void RefreshTime()
    {
        if (currentTrack == null || timeSlider == null) return;
        float t = Mathf.Clamp(currentTrack.currentTime, 0f, totalTime);
        currentTimeText.text = $"Time: {t:F2}";
        if (!Mathf.Approximately(timeSlider.value, t))
            timeSlider.value = t;
    }

    private void RefreshCamera()
    {
        if (camActiveToggle == null || currentTrack == null) return;

        camActiveToggle.isOn = currentTrack.GetPreCameraClipActive(currentTrack.currentTime);

        if (currentTrack.cameraController != null)
        {
            if (fovSlider != null)
            {
                fovSlider.value = currentTrack.cameraController.GetFOV();
                if (fovText != null) fovText.text = fovSlider.value.ToString("F0");
            }
            if (focusDistanceSlider != null)
            {
                focusDistanceSlider.value = currentTrack.cameraController.GetFocusDistance();
                if (focusDistanceValueText != null) focusDistanceValueText.text = focusDistanceSlider.value.ToString("F0");
            }
        }
    }

    // ---------------- 相机交互事件 ----------------
    private void OnCamActiveChanged(bool isActive)
    {
        if (currentTrack == null || !currentTrack.isCamera) return;

        var currentClip = currentTrack.GetClipAtTime(currentTrack.currentTime);
        if (currentClip != null)
            currentClip.isCameraActiveAtTime = isActive;

        if (CameraManager.Instance != null)
        {
            var sel = CameraManager.Instance.GetCurrentSelectedCameraController();
            if (sel != null && sel != currentTrack.cameraController)
            {
                if (isActive) CameraManager.Instance.SelectCamera(sel);
                else          CameraManager.Instance.ClearSelectedCamera(sel);
            }
        }
    }
    private void OnFOVChanged(float value)
    {
        if (currentTrack == null || !currentTrack.isCamera) return;
        currentTrack.cameraController?.SetFOV(value);
        if (fovText) fovText.text = value.ToString("F0");
    }
    private void OnFocusDistanceChanged(float value)
    {
        if (currentTrack == null || !currentTrack.isCamera) return;
        currentTrack.cameraController?.SetFocusDistance(value);
        if (focusDistanceValueText) focusDistanceValueText.text = value.ToString("F0");
    }

    // ---------------- 初始化入口 ----------------
    public void Initialize(TimelineTrack track)
    {
        currentTrack = track;
        gameObject.SetActive(true);

        if (titleText) titleText.text = "Timeline - " + track.gameObject.name;

        if (timeSlider)
        {
            timeSlider.wholeNumbers = false;
            timeSlider.minValue = 0f;
            timeSlider.maxValue = totalTime;
            timeSlider.value    = Mathf.Clamp(track.currentTime, 0f, totalTime);
        }

        // 默认页宽/总时长可以直接在 Inspector 改；运行时改用下面两个 API
        currentVisibleTime = Mathf.Clamp(defaultVisibleTime, EffectiveMinPage, EffectiveMaxPage);

        UpdateTimeline();
        RefreshAll();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
        UpdateZoomButtons();
    }

    public void RefreshAll()
    {
        if (currentTrack == null) return;
        RefreshKeyframeList();
        RefreshTime();
        if (currentTrack.isCamera) RefreshCamera();
    }

    // ---------------- 运行时参数修改 API ----------------
    /// <summary>运行时修改总时长。会自动更新 slider 区间与布局。</summary>
    public void SetTotalTime(float newTotalTime, bool clampCurrentTime = true)
    {
        totalTime = Mathf.Max(0.001f, newTotalTime);
        if (timeSlider)
        {
            timeSlider.maxValue = totalTime;
            if (clampCurrentTime)
                timeSlider.value = Mathf.Clamp(timeSlider.value, 0f, totalTime);
        }
        UpdateTimeline();
        RefreshKeyframeList();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
    }

    /// <summary>运行时设置每页可见秒数（会被阈值与安全上下限夹取）。</summary>
    public void SetVisibleTime(float visibleSeconds)
    {
        currentVisibleTime = Mathf.Clamp(visibleSeconds, EffectiveMinPage, EffectiveMaxPage);
        ClampZoom();
        UpdateTimeline();
        RefreshKeyframeList();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
        UpdateZoomButtons();
    }

    /// <summary>运行时调整可自定义阈值（例如把 10..30 改到 8..40）。会立即夹取当前页宽并刷新。</summary>
    public void SetZoomPageBounds(float minPage, float maxPage)
    {
        minPageSeconds = Mathf.Max(0.001f, minPage);
        maxPageSeconds = Mathf.Max(minPageSeconds + 0.001f, maxPage);
        // 重新应用到当前页宽
        currentVisibleTime = Mathf.Clamp(currentVisibleTime, EffectiveMinPage, EffectiveMaxPage);
        UpdateTimeline();
        RefreshKeyframeList();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
        UpdateZoomButtons();
    }

    // ---------------- 调试 ----------------
    [ContextMenu("Debug Timeline Geometry")]
    private void DebugCheck()
    {
        float viewportWidth = viewport ? viewport.rect.width : 0f;
        float pps = pixelsPerSecond;
        float contentWidth = content ? content.sizeDelta.x : 0f;
        float visibleTime  = (pps > 0f) ? viewportWidth / pps : 0f;
        float viewStartSec = GetViewStartSec();
        float viewEndSec   = viewStartSec + currentVisibleTime;

        
        
        Debug.Log(
            $"[Timeline Debug]\n" +
            $"- totalTime: {totalTime}s\n" +
            $"- currentVisibleTime(set): {currentVisibleTime}s (bounds {EffectiveMinPage}..{EffectiveMaxPage})\n" +
            $"- viewportWidth(px): {viewportWidth:F1}\n" +
            $"- pixelsPerSecond: {pps:F3}\n" +
            $"- contentWidth(px): {contentWidth:F1}\n" +
            $"- computedVisibleTime(px/pps): {visibleTime:F3}s\n" +
            $"- view window: [{viewStartSec:F2} .. {viewEndSec:F2}]\n" +
            $"- scrollbar.size: {(timelineScrollbar? timelineScrollbar.size:0f):F3}\n" +
            $"- timeSlider.value: {(timeSlider? timeSlider.value:0f):F2}"
        );
    }

    public void HidePanel() => gameObject.SetActive(false);
    public void ShowPanel() => gameObject.SetActive(true);
    
    [ContextMenu("Debug Background Rect")]
    private void DebugBackgroundRect()
    {
        if (!timelineBackground)
        {
            Debug.LogWarning("timelineBackground = null");
            return;
        }
        var bg = timelineBackground;
        Debug.Log(
            $"[BG] sizeDelta=({bg.sizeDelta.x:F1},{bg.sizeDelta.y:F1}) " +
            $"rect=({bg.rect.width:F1},{bg.rect.height:F1}) " +
            $"anchoredPos=({bg.anchoredPosition.x:F1},{bg.anchoredPosition.y:F1}) " +
            $"anchors=({bg.anchorMin})-({bg.anchorMax}) pivot={bg.pivot}"
        );
    }
}
