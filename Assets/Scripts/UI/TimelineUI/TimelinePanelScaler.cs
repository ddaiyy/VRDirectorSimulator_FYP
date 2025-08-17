using UnityEngine;
using UnityEngine.UI;

public class TimelinePanelScaler : MonoBehaviour
{
    [Header("UI 元素")]
    public RectTransform content;        // 时间轴内容
    public RectTransform viewport;       // Viewport
    public ScrollRect scrollRect;        // ScrollRect
    public Slider timelineSlider;        // Slider
    public RectTransform sliderHandle;   // Slider Handle
    //public Text timeLabel;               // 时间标签

    [Header("时间设置")]
    public float totalTime = 300f;
    public float defaultVisibleTime = 20f;
    public float zoomStep = 1.5f;
    public float minVisibleTime = 5f;
    public float maxVisibleTime = 120f;

    [Header("像素设置")]
    public float pixelsPerSecond = 20f; // 固定像素/秒

    private float currentVisibleTime;

    private void Start()
    {
        currentVisibleTime = defaultVisibleTime;
        UpdateTimeline();
    }

    public void ZoomIn()
    {
        currentVisibleTime = Mathf.Max(currentVisibleTime / zoomStep, minVisibleTime);
        UpdateTimeline();
        
        
    }

    public void ZoomOut()
    {
        currentVisibleTime = Mathf.Min(currentVisibleTime * zoomStep, maxVisibleTime);
        UpdateTimeline();
    }

    private void UpdateTimeline()
    {
        // 🔑 保持左边固定：直接改 content 宽度（右边伸缩）
        float contentWidth = totalTime * pixelsPerSecond;
        content.sizeDelta = new Vector2(contentWidth, content.sizeDelta.y);

        // 固定左对齐
        content.anchorMin = new Vector2(0f, content.anchorMin.y);
        content.anchorMax = new Vector2(0f, content.anchorMax.y);
        content.anchoredPosition = new Vector2(0, content.anchoredPosition.y);

        // Slider 总长度用固定的基准宽度（比如 viewport 宽度）
        RectTransform sliderRect = timelineSlider.GetComponent<RectTransform>();
        float sliderTotalWidth = sliderRect.sizeDelta.x;

        // 不用 currentVisibleTime/totalTime，而是直接用缩放后的像素密度
        float handleWidth = defaultVisibleTime / currentVisibleTime * sliderTotalWidth;
        sliderHandle.sizeDelta = new Vector2(handleWidth, sliderHandle.sizeDelta.y);


        // Handle 左对齐
        sliderHandle.anchoredPosition = new Vector2(handleWidth / 2f, sliderHandle.anchoredPosition.y);

        // 更新 Label
        //UpdateLabel();
    }

    /*private void UpdateLabel()
    {
        timeLabel.text = $"当前显示时间长度: {currentVisibleTime:F1}s / 总长: {totalTime}s\n" +
                         $"起始时间: 0s";
    }*/
}
