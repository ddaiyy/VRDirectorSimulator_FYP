using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MasterTimelineUI : MonoBehaviour
{
    public static MasterTimelineUI Instance { get; private set; }

    [Header("UI References")]
    public RectTransform timelineContent; // 拖到Content
    public GameObject trackBarPrefab;     // 拖到你做好的Prefab
    public RectTransform objectNameContent; //物体名字区域
    public GameObject objectNamePrefab;     //物体名字条目Prefab（带TextMeshProUGUI）
    public Button playButton;
    public Button stopButton;
    
    
    [Header("Timeline Settings")]
    public float timelineWidth = 40f;   // 时间轴总宽度
    public float timelineHeight = 40f;    // 条带高度
    public float barSpacing = 10f;        // 条带间距

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        RefreshTimelineUI();
        playButton.onClick.AddListener(OnPlayClicked);
        stopButton.onClick.AddListener(OnStopClicked);
    }

    private void OnStopClicked()
    {
        TimelineManager.Instance.masterTrack.Stop();
    }

    private void OnPlayClicked()
    {
        TimelineManager.Instance.masterTrack.Play();
    }

    public void RefreshTimelineUI()
    {
        foreach (Transform child in timelineContent)
            Destroy(child.gameObject);
        if (objectNameContent != null)
        {
            foreach (Transform child in objectNameContent)
                Destroy(child.gameObject);
        }

        var tracks = TimelineManager.Instance?.GetAllTracks();
        if (tracks == null || tracks.Count == 0) return;

        // 计算主时间轴总时长（只考虑有关键帧的轨道）
        float maxDuration = tracks.Where(t => t.clips != null && t.clips.Count > 0)
                                  .Select(t => t.GetDuration())
                                  .DefaultIfEmpty(1f).Max();
        if (maxDuration <= 0f) maxDuration = 1f;

        for (int i = 0; i < tracks.Count; i++)
        {
            var track = tracks[i];
            float startTime = 0f;
            float duration = 0.1f; // 默认最小宽度
            bool hasClips = track.clips != null && track.clips.Count > 0;
            if (hasClips)
            {
                startTime = track.clips.Min(c => c.time);
                duration = track.GetDuration();
                if (duration <= 0f) duration = 0f;
            }

            // 只在有关键帧且duration>0时显示条带
            if (hasClips && duration > 0f)
            {
                float x = (startTime / maxDuration) * timelineWidth;
                float width = (duration / maxDuration) * timelineWidth;
                if (width < 10f) width = 10f; // 最小宽度

                GameObject bar = Instantiate(trackBarPrefab, timelineContent);
                RectTransform barRect = bar.GetComponent<RectTransform>();
                barRect.anchorMin = new Vector2(0, 1);
                barRect.anchorMax = new Vector2(0, 1);
                barRect.pivot = new Vector2(0, 1);
                barRect.anchoredPosition = new Vector2(x, -i * (timelineHeight + barSpacing));
                barRect.sizeDelta = new Vector2(width, timelineHeight);

                var label = bar.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = track.gameObject.name;
            }

            // 始终显示物体名字条目
            if (objectNameContent != null && objectNamePrefab != null)
            {
                GameObject nameItem = Instantiate(objectNamePrefab, objectNameContent);
                RectTransform nameRect = nameItem.GetComponent<RectTransform>();
                nameRect.anchorMin = new Vector2(0, 1);
                nameRect.anchorMax = new Vector2(0, 1);
                nameRect.pivot = new Vector2(0, 1);
                nameRect.anchoredPosition = new Vector2(0, -i * (timelineHeight + barSpacing));
                nameRect.sizeDelta = new Vector2(objectNameContent.sizeDelta.x, timelineHeight);
                var nameLabel = nameItem.GetComponentInChildren<TextMeshProUGUI>();
                if (nameLabel != null)
                    nameLabel.text = track.gameObject.name;
            }
        }

        timelineContent.sizeDelta = new Vector2(timelineWidth, tracks.Count * (timelineHeight + barSpacing));
        if (objectNameContent != null)
            objectNameContent.sizeDelta = new Vector2(objectNameContent.sizeDelta.x, tracks.Count * (timelineHeight + barSpacing));
    }
}
