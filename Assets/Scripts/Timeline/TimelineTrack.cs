using System;
using System.Collections.Generic;
using UnityEngine;

public class TimelineTrack : MonoBehaviour
{
    public List<TimelineClip> clips = new List<TimelineClip>();
    public float currentTime = 0f;
    public bool isPlaying = false;
    public float duration = 5f; // 轨道总时长，可根据clips自动计算

    void Update()
    {
        if (isPlaying && clips.Count > 1)
        {
            currentTime += Time.deltaTime;
            if (currentTime > duration)
            {
                currentTime = duration;
                isPlaying = false; // 播放完毕
                SetTime(0f);
            }
            ApplyClipAtTime(currentTime);
        }
    }

    [ContextMenu("添加关键帧")]
    public void AddClip()
    {
        TimelineClip clip = new TimelineClip();
        clip.time = currentTime; //TODO：暂时需自定义
        clip.position = transform.position;
        clip.rotation = transform.rotation;
        clip.scale = transform.localScale;
        Camera cam = GetComponent<Camera>();
        if (cam != null)
            clip.fov = cam.fieldOfView;
        else
            clip.fov = 60f;
        clips.Add(clip);
    }
    
    [ContextMenu("播放")]
    public void Play()
    {
        isPlaying = true;
        currentTime = 0f;
        duration = GetDuration();
    }

    [ContextMenu("停止")]
    public void Stop()
    {
        isPlaying = false;
        currentTime = 0f;
    }

    // 平滑插值
    void ApplyClipAtTime(float time)
    {
        // 找到前后两个关键帧
        TimelineClip prev = null, next = null;
        for (int i = 0; i < clips.Count - 1; i++)
        {
            if (clips[i].time <= time && clips[i + 1].time >= time)
            {
                prev = clips[i];
                next = clips[i + 1];
                break;
            }
        }
        if (prev == null || next == null) return;

        float t = (time - prev.time) / (next.time - prev.time);

        transform.position = Vector3.Lerp(prev.position, next.position, t);
        transform.rotation = Quaternion.Slerp(prev.rotation, next.rotation, t);
        transform.localScale = Vector3.Lerp(prev.scale, next.scale, t);
        
        //TODO: 摄像机
        Camera cam = GetComponent<Camera>();
        if (cam != null)
            cam.fieldOfView = Mathf.Lerp(prev.fov, next.fov, t);
    }
    
    public float GetDuration()
    {
        if (clips.Count == 0) return 0f;
        float maxTime = 0f;
        foreach (var clip in clips)
        {
            if (clip.time > maxTime)
                maxTime = clip.time;
        }
        return maxTime;
    }

    public void SetTime(float time)
    {
        currentTime = time;
        ApplyClipAtTime(time);
    }
} 