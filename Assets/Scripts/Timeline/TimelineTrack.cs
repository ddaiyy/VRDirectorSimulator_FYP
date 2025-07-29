using System.Collections.Generic;
using UnityEngine;

public class TimelineTrack : MonoBehaviour
{
    public GameObject target;
    public List<TimelineClip> clips = new List<TimelineClip>();
    public bool isPlaying = false;
    public bool isPaused = false;
    public float currentTime = 0f;

    //添加关键帧
    public void AddClip(TimelineClip clip) { /*TODO*/ }
    
    //播放轨道
    public void Play() { /*TODO*/ }
    
    //停止轨道
    public void Stop() { /*TODO*/ }
    
    //暂停轨道
    public void Pause() { /*TODO*/ }
    
    //预览
    public void Preview() { /*TODO*/ }
    
    //获取轨道时长
    public float GetDuration() { /*TODO*/ return 0f; }
    
    //获取关键帧数量
    public int GetClipCount() { return clips.Count; }
    
    //清空所有关键帧
    public void ClearClips() { /*TODO*/ }
} 