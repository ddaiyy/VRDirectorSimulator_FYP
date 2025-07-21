using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TesTimelineUI : MonoBehaviour
{
    [Header("UI输入")]
    public string searchName = "";
    //public string currentObjName = "";
    public GameObject currentObj;
    private List<GameObject> allTrackObjects = new List<GameObject>();
    private List<string> filteredNames = new List<string>();
    private int selectedIndex = -1;
    private TimelineTrack lastSelectedTrack = null; // 记录上一个选中的轨道

    public ObjectTimelineUI timelinePanel;

    void Start()
    {
        // 收集所有带TimelineTrack的物体
        allTrackObjects = FindObjectsOfType<TimelineTrack>().Select(t => t.gameObject).ToList();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200), GUI.skin.box);
        GUILayout.Label("输入物体名选择查找时间轴：");

        // 输入框
        string newSearch = GUILayout.TextField(searchName);
        if (newSearch != searchName)
        {
            searchName = newSearch;
            UpdateFilteredList();
        }

        // 下拉选择
        if (filteredNames.Count > 0)
        {
            for (int i = 0; i < filteredNames.Count; i++)
            {
                if (GUILayout.Button(filteredNames[i]))
                {
                    SelectByName(filteredNames[i]);
                }
            }
        }
        else if (!string.IsNullOrEmpty(searchName))
        {
            GUILayout.Label("没有匹配的物体");
        }

        GUILayout.EndArea();
    }

    void UpdateFilteredList()
    {
        filteredNames = allTrackObjects
            .Select(obj => obj.name)
            .Where(name => name.ToLower().Contains(searchName.ToLower()))
            .ToList();
    }

    void SelectByName(string name)
    {
        GameObject obj = allTrackObjects.FirstOrDefault(o => o.name == name);
        if (obj != null)
        {
            TimelineTrack track = obj.GetComponent<TimelineTrack>();
            if (track != null)
            {
                // 关闭上一个
                if (lastSelectedTrack != null && lastSelectedTrack != track)
                {
                    HideTimelineUI(lastSelectedTrack);
                }

                // 显示新的
                ShowTimelineUI(track);
                currentObj = obj;
                lastSelectedTrack = track;
            }
        }
    }

    // UI显示逻辑
    public void ShowTimelineUI(TimelineTrack track)
    {
        Debug.Log("显示 " + track.gameObject.name + " 的时间轴UI");
        // 这里写你的UI刷新逻辑
        timelinePanel.ShowPanel(track);
    }

    // 关闭UI逻辑
    public void HideTimelineUI(TimelineTrack track)
    {
        Debug.Log("关闭 " + track.gameObject.name + " 的时间轴UI");
        // 这里写你的UI隐藏逻辑
        timelinePanel.HidePanel();
    }
}
