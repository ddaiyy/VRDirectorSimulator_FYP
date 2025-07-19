using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterTimelineTrack : MonoBehaviour
{
    public float currentTime = 0f;
    public float duration = 0f;
    public bool isPlaying = false;

    void Update()
    {
        if (isPlaying)
        {
            currentTime += Time.deltaTime;
            if (currentTime > duration)
            {
                currentTime = duration;
                foreach (var track in TimelineManager.Instance.GetAllTracks())
                {
                    if (track.isControlledByMaster){
                        track.SetTime(track.clips[0].time);
                    }
                }
                
                isPlaying = false;
                foreach (var track in TimelineManager.Instance.GetAllTracks())
                {
                    if (track.isControlledByMaster){
                        track.isControlledByMaster = false;
                    }
                }
                
            }else{
                // 推进所有轨道
                foreach (var track in TimelineManager.Instance.GetAllTracks())
                {
                    if (track.isControlledByMaster){
                        track.SetTime(currentTime);
                    }
                }
            }
            
        }
    }

    [ContextMenu("播放")]
    public void Play()
    {
        isPlaying = true;
        currentTime = 0f;
        duration = GetDuration();
        
        foreach (var track in TimelineManager.Instance.GetAllTracks())
        {
            track.isControlledByMaster = true;
        }
    }

    [ContextMenu("暂停")]
    public void Pause()
    {
        isPlaying = false;
    }

    [ContextMenu("停止")]
    public void Stop()
    {
        isPlaying = false;
        currentTime = 0f;
        foreach (var track in TimelineManager.Instance.GetAllTracks())
        {
            track.SetTime(track.clips[0].time);
            track.isControlledByMaster = false;
        }
    }

    // 计算所有轨道的最大时长
    public float GetDuration()
    {
        float max = 0f;
        foreach (var track in TimelineManager.Instance.GetAllTracks())
        {
            float d = track.GetDuration();
            if (d > max) max = d;
        }
        return max;
    }
}
