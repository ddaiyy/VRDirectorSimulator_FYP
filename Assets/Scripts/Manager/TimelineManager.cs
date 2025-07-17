using System.Collections.Generic;
using UnityEngine;

public interface ITimelineManager
{
    void AddTrack(TimelineTrack track);
    void RemoveTrack(TimelineTrack track);
    void PlayAll();
    void StopAll();
    void PauseAll();
    void PreviewTrack(TimelineTrack track);
    List<TimelineTrack> GetTracks();
    TimelineTrack GetTrackByTarget(GameObject target);
    void ClearAllTracks();
    void ExportTimelineData();
    void ImportTimelineData();
}

public class TimelineManager : MonoBehaviour, ITimelineManager
{
    public List<TimelineTrack> tracks = new List<TimelineTrack>();

    //添加轨道
    public void AddTrack(TimelineTrack track) { /* TODO */ }
    
    //移除轨道
    public void RemoveTrack(TimelineTrack track) { /*TODO*/ }
    
    //播放所有轨道
    public void PlayAll() { /*TODO*/ }
    
    //停止所有轨道
    public void StopAll() { /*TODO*/ }
    
    //暂停所有轨道
    public void PauseAll() { /*TODO*/ }
    
    //预览指定轨道
    public void PreviewTrack(TimelineTrack track) { /*TODO*/ }
    
    //获取所有轨道
    public List<TimelineTrack> GetTracks() => tracks;
    
    //获取指定物体轨道
    public TimelineTrack GetTrackByTarget(GameObject target) { /*TODO*/ return null; }
    
    //清除所有轨道
    public void ClearAllTracks() { /*TODO*/ }
    
    //导出时间轴数据
    public void ExportTimelineData() { /*TODO*/ }
    
    //导入时间轴数据
    public void ImportTimelineData() { /*TODO*/ }
} 