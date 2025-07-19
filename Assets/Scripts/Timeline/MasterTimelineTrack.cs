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
                isPlaying = false;
            }
            // 推进所有轨道
            foreach (var track in TimelineManager.Instance.GetAllTracks())
            {
                track.SetTime(currentTime);
            }
            // 如果主轨道自己也有clips，可以在这里处理
            // ApplyClipAtTime(currentTime);
        }
    }

    [ContextMenu("播放")]
    public void Play()
    {
        isPlaying = true;
        currentTime = 0f;
        duration = GetDuration();
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
            track.SetTime(0f);
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
