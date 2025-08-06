using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Added for .Select()

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
                    if (track.isControlledByMaster)
                    {
                        // 检查clips是否为空，避免IndexOutOfRangeException
                        if (track.clips != null && track.clips.Count > 0)
                        {
                            track.SetTime(track.clips[0].time);
                        }
                        else
                        {
                            // 如果轨道没有关键帧，设置为0秒
                            track.SetTime(0f);
                        }
                    }
                }

                foreach (var track in TimelineManager.Instance.GetAllTracks())
                {
                    if (track.isControlledByMaster)
                    {
                        track.isControlledByMaster = false;
                    }
                }

                isPlaying = false;
            }
            else
            {
                // 推进所有轨道
                foreach (var track in TimelineManager.Instance.GetAllTracks())
                {
                    if (track.isControlledByMaster){
                        track.SetTime(currentTime);
                    }
                }
                
                // 检测相机冲突
                var conflictingCameras = CameraManager.Instance.CheckCameraConflictAtTime(currentTime);
                if (conflictingCameras.Count > 1)
                {
                    // 有冲突，停止播放并报错
                    string cameraNames = string.Join(", ", conflictingCameras.Select(c => c.gameObject.name));
                    Debug.LogError($"[MasterTimelineTrack] 检测到相机冲突！时间: {currentTime:F2}s，冲突相机: {cameraNames}");
                    
                    // 停止播放
                    isPlaying = false;
                    foreach (var track in TimelineManager.Instance.GetAllTracks())
                    {
                        if (track.isControlledByMaster)
                        {
                            track.isControlledByMaster = false;
                            track.SetTime(track.clips[0].time);
                        }
                    }
                    return; // 不执行后续的轨道更新
                }else if (conflictingCameras.Count == 0)
                {
                    //TODO:清空黑
                }
                else if(conflictingCameras.Count == 1)
                {
                    if (CameraManager.Instance.currentSelected != conflictingCameras.First())
                    {
                        CameraManager.Instance.ClearSelectedCamera(CameraManager.Instance.currentSelected);
                        CameraManager.Instance.SelectCamera(conflictingCameras.First());
                    }
                    else
                    {
                        //A在播放，A激活
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
        //ClearPreviewTexture();
        
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
            // 检查clips是否为空，避免IndexOutOfRangeException
            if (track.clips != null && track.clips.Count > 0)
            {
                track.SetTime(track.clips[0].time);
            }
            else
            {
                // 如果轨道没有关键帧，设置为0秒
                track.SetTime(0f);
            }
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