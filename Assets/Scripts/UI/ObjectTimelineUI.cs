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
    public float minVisibleTime = 5f;
    public float maxVisibleTime = 120f;

    [Header("几何与缓冲")]
    public float rightEdgePaddingPx = 24f;  // Content 右侧缓冲像素，避免滚到尽头顶死
    public float topRowGapPx = 0f;          // 行整体向下偏移（上缘坐标系，向下为负）

    [Header("安全选项")]
    public bool autoFixConflictingLayoutOnContent = true;

    [Header("背景条（覆盖所有 Content）")]
    public RectTransform timelineBackground;  // 把你那条整段背景图拖进来

    // 运行态
    private float currentVisibleTime;
    private float pixelsPerSecond;          // pps = viewport.width / currentVisibleTime
    [SerializeField] private TimelineTrack currentTrack;

    private float _lastSetContentWidth = -1f;
    private bool _programmaticScroll = false;

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

        currentVisibleTime = Mathf.Clamp(defaultVisibleTime, minVisibleTime, maxVisibleTime);

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
        // 每秒像素数增加，单位时间占更多像素 → 页面能看到的时间变少
        pixelsPerSecond *= zoomStep;
        ClampZoom();
        UpdateTimeline();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
    }

    public void ZoomOut()
    {
        // 每秒像素数减少，单位时间占更少像素 → 页面能看到的时间变多
        pixelsPerSecond /= zoomStep;
        ClampZoom();
        UpdateTimeline();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
    }

    // 保证缩放范围在 minVisibleTime ~ maxVisibleTime 之间
    private void ClampZoom()
    {
        float viewportWidth = viewport ? viewport.rect.width : 0f;
        if (viewportWidth <= 0f) return;

        // 计算当前一页能显示多少秒
        currentVisibleTime = viewportWidth / pixelsPerSecond;

        if (currentVisibleTime < minVisibleTime)
        {
            currentVisibleTime = minVisibleTime;
            pixelsPerSecond = viewportWidth / currentVisibleTime;
        }
        else if (currentVisibleTime > maxVisibleTime)
        {
            currentVisibleTime = maxVisibleTime;
            pixelsPerSecond = viewportWidth / currentVisibleTime;
        }
    }

    private void UpdateTimeline()
    {
        Canvas.ForceUpdateCanvases();
        float viewportWidth = viewport ? viewport.rect.width : 0f;
        if (viewportWidth <= 0f) return;

        // 强制比例：一页（viewport） = currentVisibleTime 秒
        pixelsPerSecond = viewportWidth / Mathf.Max(currentVisibleTime, 0.0001f);

        // Content 宽：总时长 * pps + 右侧缓冲
        float contentWidth = totalTime * pixelsPerSecond + Mathf.Max(rightEdgePaddingPx, 0f);
        SetContentWidth(contentWidth);
        
        if (timelineBackground)
        {
            // 建议背景锚点与 Content 一致：anchorMin=(0,1), anchorMax=(0,1), pivot=(0,1)
            var bg = timelineBackground;
            bg.sizeDelta = new Vector2(contentWidth, bg.sizeDelta.y);
            bg.anchoredPosition = new Vector2(0f, bg.anchoredPosition.y);
        }
        
        // Slider 轨道长度 = Content 宽（保持对齐）
        if (timeSlider)
        {
            var rt = timeSlider.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(contentWidth, rt.sizeDelta.y);
            rt.anchoredPosition = new Vector2(0f, -topRowGapPx); // 与行对齐（上缘坐标系：向下为负）
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
    private void EnsureTimeVisible(float t)
    {
        if (!timelineScrollbar) return;

        float viewportWidth = viewport.rect.width;
        float contentWidth  = content.sizeDelta.x;
        float pps           = Mathf.Max(pixelsPerSecond, 0.0001f);

        float viewStartSec = Mathf.Max(-content.anchoredPosition.x / pps, 0f);
        float viewEndSec   = viewStartSec + currentVisibleTime;
        if (t >= viewStartSec && t <= viewEndSec) return;

        float targetStart = Mathf.Clamp(t - currentVisibleTime * 0.5f, 0f, Mathf.Max(totalTime - currentVisibleTime, 0f));
        float desiredOffsetX = targetStart * pps;
        float scrollRangePx  = Mathf.Max(contentWidth - viewportWidth, 0f);
        desiredOffsetX       = Mathf.Clamp(desiredOffsetX, 0f, scrollRangePx);

        float size  = Mathf.Clamp01(timelineScrollbar.size);
        float range = Mathf.Max(1f - size, 0.0001f);
        float nComp = (scrollRangePx <= 0f) ? 0f : desiredOffsetX / scrollRangePx;
        float value = Mathf.Clamp01(nComp * range);

        _programmaticScroll = true;
        timelineScrollbar.value = value; // 会触发 OnTimelineScroll
        _programmaticScroll = false;
    }

    // ---------------- 通用：将时间点安全放入任意 Track 行 ----------------
    /// <summary>
    /// 将时间 t 放置到 trackContent 的局部坐标中（自动做 Content→世界→Track 的坐标转换），
    /// 避免锚点/缩放/左侧标签等结构造成的偏移，保证与 timeSlider.Handle 垂直对齐。
    /// </summary>
    private void PlacePointAtTime(RectTransform point, RectTransform trackContent, float t)
    {
        float xInContent = t * Mathf.Max(pixelsPerSecond, 0.0001f);     // 时间在 Content 坐标系的 X
        Vector3 world = content.TransformPoint(new Vector3(xInContent, 0f, 0f)); // Content→世界
        Vector3 localInTrack = trackContent.InverseTransformPoint(world);         // 世界→Track局部
        point.anchoredPosition = new Vector2(localInTrack.x, 0f);
    }

    // ---------------- 关键帧渲染/对齐 ----------------
    private float TimeToX(float t) => t * Mathf.Max(pixelsPerSecond, 0.0001f); // 仍可用在自定义绘制里

    private void RefreshKeyframeList()
    {
        foreach (Transform child in keyframeListContent)
            Destroy(child.gameObject);

        // 你想展示的轨道（相机多两个）
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

            // 每个关键帧：用坐标转换放置，免疫预制结构差异
            if (currentTrack != null)
            {
                foreach (var clip in currentTrack.clips)
                {
                    GameObject point = Instantiate(keyframeItemPrefab, trackContent);
                    RectTransform rect = point.GetComponent<RectTransform>();

                    // ✅ 坐标转换，保证与 Handle 垂直对齐
                    PlacePointAtTime(rect, trackContent, clip.time);

                    var text = point.GetComponentInChildren<TextMeshProUGUI>();
                    if (text) text.text = $"{clip.time:F2}";

                    var btn = point.GetComponent<Button>();
                    if (btn != null)
                    {
                        float t = clip.time;
                        btn.onClick.AddListener(() =>
                        {
                            // 点击关键帧 → 跳到该时间（并保证可见）
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
        RefreshKeyframeList(); // 新点自动落在 Handle 正上方（同一 x）
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

        UpdateTimeline();
        RefreshAll();
        EnsureTimeVisible(timeSlider ? timeSlider.value : 0f);
    }

    public void RefreshAll()
    {
        if (currentTrack == null) return;
        RefreshKeyframeList();
        RefreshTime();
        if (currentTrack.isCamera) RefreshCamera();
    }

    // ---------------- 调试 ----------------
    [ContextMenu("Debug Timeline Geometry")]
    private void DebugCheck()
    {
        float viewportWidth = viewport ? viewport.rect.width : 0f;
        float pps = pixelsPerSecond;
        float contentWidth = content ? content.sizeDelta.x : 0f;
        float visibleTime  = (pps > 0f) ? viewportWidth / pps : 0f;
        float viewStartSec = (pps > 0f) ? Mathf.Max(-content.anchoredPosition.x / pps, 0f) : 0f;
        float viewEndSec   = viewStartSec + currentVisibleTime;

        Debug.Log(
            $"[Timeline Debug]\n" +
            $"- totalTime: {totalTime}s\n" +
            $"- currentVisibleTime(set): {currentVisibleTime}s\n" +
            $"- viewportWidth(px): {viewportWidth:F1}\n" +
            $"- pixelsPerSecond: {pps:F3}\n" +
            $"- contentWidth(px): {contentWidth:F1}\n" +
            $"- computedVisibleTime(px/pps): {visibleTime:F3}s\n" +
            $"- view window: [{viewStartSec:F2} .. {viewEndSec:F2}]\n" +
            $"- scrollbar.size: {(timelineScrollbar? timelineScrollbar.size:0f):F3}\n" +
            $"- timeSlider.value: {(timeSlider? timeSlider.value:0f):F2}"
        );
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }
}
