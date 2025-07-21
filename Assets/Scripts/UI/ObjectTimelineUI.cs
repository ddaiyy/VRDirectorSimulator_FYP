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
    private TimelineTrack currentTrack;

    public RectTransform timelineContent; // 拖到Inspector
    public float timelineWidth = 600f; // 你的时间轴UI宽度
    public float maxTime = 20f; // 时间轴最大时间

    void Start()
    {
        // 绑定按钮事件
        playButton.onClick.AddListener(OnPlayClicked);
        stopButton.onClick.AddListener(OnStopClicked);
        timeSlider.onValueChanged.AddListener(OnSliderChanged);
        addKeyframeButton.onClick.AddListener(OnAddKeyframeClicked);

        HidePanel();
    }

    public void ShowPanel(TimelineTrack track)
    {
        currentTrack = track;
        gameObject.SetActive(true);

        titleText.text = "Timeline- " + track.gameObject.name;
        timeSlider.minValue = 0;
        timeSlider.maxValue = 20f;
        timeSlider.value = track.currentTime;

        RefreshKeyframeList();
        RefreshTime();
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
        currentTrack = null;
    }

    void Update()
    {
        if (currentTrack != null && gameObject.activeSelf)
        {
            // 只有在播放时才自动刷新Slider
            if (currentTrack.isPlaying)
            {
                RefreshTime();
            }
        }
    }

    void RefreshTime()
    {
        currentTimeText.text = $"Time: {currentTrack.currentTime:F2} / {currentTrack.GetDuration():F2}";
        timeSlider.value = currentTrack.currentTime;
    }

    void RefreshKeyframeList()
    {
        /*// 清空旧的
        foreach (Transform child in timelineContent)
            Destroy(child.gameObject);

        foreach (var clip in currentTrack.clips)
        {
            GameObject point = Instantiate(keyframeItemPrefab, timelineContent);
            float normalizedTime = Mathf.Clamp01(clip.time / maxTime);
            float x = normalizedTime * timelineWidth;

            var rect = point.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(x, 0); // 只设置X，Y可以固定

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
                    currentTrack.SetTime(t);
                    RefreshTime();
                });
            }
        }*/

        // 清空旧的
        foreach (Transform child in keyframeListContent)
            Destroy(child.gameObject);

        // 你要支持的属性
        string[] properties = { "Position", "Rotation", "Scale" };
// 如果是摄像机，加上FOV
        if (currentTrack.GetComponent<Camera>() != null)
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
                        currentTrack.SetTime(t);
                        RefreshTime();
                    });
                }
            }
        }
    }

    void OnPlayClicked()
    {
        if (currentTrack != null)
            currentTrack.Play();
    }

    void OnStopClicked()
    {
        if (currentTrack != null)
            currentTrack.Stop();
    }

    void OnSliderChanged(float value)
    {
        if (currentTrack != null)
            currentTrack.SetTime(value);
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
        // 创建新关键帧
        TimelineClip clip = new TimelineClip();
        clip.time = time;
        clip.position = track.transform.position;
        clip.rotation = track.transform.rotation;
        clip.scale = track.transform.localScale;

        Camera cam = track.GetComponent<Camera>();
        if (cam != null)
            clip.fov = cam.fieldOfView;
        else
            clip.fov = 60f;
        track.clips.Add(clip);

        // 关键帧排序
        track.clips = track.clips.OrderBy(c => c.time).ToList();
    }
}