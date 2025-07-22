using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


public class TimelineTrack : MonoBehaviour
{
    public List<TimelineClip> clips = new List<TimelineClip>();
    public float currentTime = 0f;
    public bool isPlaying = false;
    public float duration = 0f; // 轨道总时长，可根据clips自动计算
    public bool isControlledByMaster = false;
    public PostProcessVolume volume;  // Inspector 里拖入
    private DepthOfField dof;

    void Start()
    {
        if (volume != null)
        {
            if (!volume.profile.TryGetSettings(out dof))
            {
                Debug.LogWarning("PostProcessVolume中没有DepthOfField");
            }
        }
        else
        {
            Debug.LogWarning("未设置PostProcessVolume引用！");
        }
    }

    void Update()
    {
        if (isControlledByMaster){
            return;
        }

        if (isPlaying && clips.Count > 1)
        {
            currentTime += Time.deltaTime;
            if (currentTime > duration)
            {
                currentTime = duration;
                isPlaying = false; // 播放完毕
                SetTime(clips[0].time);
            }else{
                ApplyClipAtTime(currentTime);
            }
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
        {
            clip.fov = cam.fieldOfView;

            if (dof != null)
            {
                clip.focusDistance = dof.focusDistance.value;
            }
            else
            {
                clip.focusDistance = 5f;
            }
        }
        else
        {
            clip.fov = 60f;
            clip.focusDistance = 5f;
        }

        // 记录当前激活的摄像机ID（假设用 InstanceID）
        if (CameraManager.Instance != null && CameraManager.Instance.GetCurrentSelectedCamera() != null)
        {
            clip.activeCameraID = CameraManager.Instance.GetCurrentSelectedCamera().GetInstanceID();
        }
        else
        {
            clip.activeCameraID = -1; // 没有选中摄像机
        }

        clips.Add(clip);
        
        if (clips.Count >1)
        {
            Debug.Log("计算duration");
            duration = GetDuration();
        }
        MasterTimelineUI.Instance?.RefreshTimelineUI();
    }

    public void AddClip(float time)
    {
        TimelineClip clip = new TimelineClip();
        clip.time = time;
        clip.position = transform.position;
        clip.rotation = transform.rotation;
        clip.scale = transform.localScale;
        Camera cam = GetComponent<Camera>();
        if (cam != null)
            clip.fov = cam.fieldOfView;
        else
            clip.fov = 60f;
        clips.Add(clip);
        
        if (clips.Count >1)
        {
            Debug.Log("计算duration");
            duration = GetDuration();
        }
        MasterTimelineUI.Instance?.RefreshTimelineUI();
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
        if (clips.Count == 0) return;

        Camera cam = GetComponent<Camera>();
        if (time < clips[0].time && isPlaying)
        {
            transform.position = new Vector3(99999, 99999, 99999);
            return;
        }


        if (isControlledByMaster && time >= clips[clips.Count - 1].time)
        {
            var last = clips[clips.Count - 1];
            transform.position = last.position;
            transform.rotation = last.rotation;
            transform.localScale = last.scale;
            if (cam != null)
            {
                cam.fieldOfView = last.fov;

                if (dof != null)
                {
                    dof.focusDistance.value = last.focusDistance;
                }
            }
            return;
        }

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

        /*if (prev == null || next == null) return;*/
        if (prev == null || next == null)
        {
            // 处理边界，直接取最后一个关键帧
            prev = clips[clips.Count - 1];
            next = prev;
        }

        float t = (time - prev.time) / (next.time - prev.time);

        transform.position = Vector3.Lerp(prev.position, next.position, t);
        transform.rotation = Quaternion.Slerp(prev.rotation, next.rotation, t);
        transform.localScale = Vector3.Lerp(prev.scale, next.scale, t);
        
        //TODO: 摄像机
        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(prev.fov, next.fov, t);

            if (dof != null)
            {
                dof.focusDistance.value = Mathf.Lerp(prev.focusDistance, next.focusDistance, t);
            }
        }

        // 处理摄像机激活状态：优先使用 prev 关键帧的激活摄像机
        int activeID = prev.activeCameraID;

        // 找到对应的 CameraController
        CameraController toActivate = null;
        foreach (var camCtrl in CameraManager.Instance.GetAllCameras()) // 需要你CameraManager增加GetAllCameras接口返回List<CameraController>
        {
            if (camCtrl.GetInstanceID() == activeID)
            {
                toActivate = camCtrl;
                break;
            }
        }

        // 激活选中的摄像机，关闭其他摄像机预览
        if (toActivate != null)
        {
            CameraManager.Instance.SelectCamera(toActivate);
        }
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