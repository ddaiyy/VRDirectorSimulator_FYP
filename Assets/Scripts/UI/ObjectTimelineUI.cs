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
    
    [Header("Camera相关")]
    public Toggle camActiveToggle;
    public Slider fovSlider;
    public Text fovText;
    public Slider dofSlider;
    public Text dofText;
    
    
    [Header("对应TimelineObject")]
    [SerializeField] private TimelineTrack currentTrack;

    void Start()
    {
        // 绑定按钮事件
        playButton.onClick.AddListener(OnPlayClicked);
        stopButton.onClick.AddListener(OnStopClicked);
        timeSlider.onValueChanged.AddListener(OnSliderChanged);
        addKeyframeButton.onClick.AddListener(OnAddKeyframeClicked);
        
        //fov and dof
        /*fovSlider.onValueChanged.AddListener(OnFOVChanged);
        dofSlider.minValue = 0.1f;
        dofSlider.maxValue = 10f;
        dofSlider.onValueChanged.AddListener(OnDofChanged);*/
        
        //SelectCamera
        //camActiveToggle.onValueChanged.AddListener(OnCamActiveChanged);
        //HidePanel();
    }

    // private void OnDofChanged(float arg0)
    // {
    //     
    // }
    //
    // private void OnFOVChanged(float value)
    // {
    //     if (CameraManager.Instance != null)
    //     {
    //         CameraController selected = CameraManager.Instance.GetCurrentSelectedCamera();
    //         if (selected != null)
    //         {
    //             selected.SetFOV(value);
    //         }
    //     }
    //
    //     if (fovText != null)
    //     {
    //         fovText.text = value.ToString("F0");
    //     }
    // }
    // public void SyncSlider(CameraController controller)
    // {
    //     if (controller != null && fovSlider != null)
    //     {
    //         fovSlider.value = controller.GetFOV();
    //         if (fovText != null)
    //         {
    //             fovText.text = controller.GetFOV().ToString("F0");
    //         }
    //     }
    // }
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
    }

    void Update()
    {
        if (currentTrack != null && gameObject.activeSelf)
        {
            RefreshTime();
        }
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
        track.AddClip(time);
    }

    public void Initialize(TimelineTrack track)
    {
        currentTrack = track; // 保存对 TimelineTrack 的引用

        gameObject.SetActive(true); // 激活UI
        titleText.text = "Timeline- " + track.gameObject.name; // 设置标题
        timeSlider.minValue = 0; // 设置时间轴范围
        timeSlider.maxValue = 20f;
        timeSlider.value = track.currentTime; // 设置当前时间

        RefreshKeyframeList(); // 刷新关键帧列表
        RefreshTime(); // 刷新时间显示
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
    }
}