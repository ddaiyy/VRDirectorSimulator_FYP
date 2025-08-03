using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;


public class TimelineTrack : MonoBehaviour
{
    public List<TimelineClip> clips = new List<TimelineClip>();
    public float currentTime = 0f;
    public bool isPlaying = false;
    public float duration = 0f; // 轨道总时长，可根据clips自动计算
    public bool isControlledByMaster = false;
    public PostProcessVolume volume; // Inspector 里拖入
    private DepthOfField dof;
    public bool isCameraActive = false;

    [Header("UI相关配置")] public ObjectTimelineUI objectTimelineUI;
    public GameObject objectTimelineUIPrefab;
    public GameObject cameraTimelineUIPrefab;

    [Header("default 开始配置")] public Vector3 startPosition;
    public Quaternion startRotation;
    public float fov = 60f;
    public Vector3 startScale;


    private Camera cam;
    public bool isCamera = false;

    [Header("测试用参数")] public string animName = "Test Animation Name";
    public float animDuration = 5f;
    [SerializeField] private bool isAnimPlaying = false;
    private bool lastCameraActiveState = false; // 记录上次的相机激活状态，避免重复切换

    void Start()
    {
        //TODO:可以换到检查相机那里面去
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

        //初始化UI
        CheckCameraExists();
        InitializeData();
        InitializeTimelineUI();
    }

    void InitializeData()
    {
        //TODO:把生成时候的位置数据变成默认数据
    }

    // 获取当前相机激活状态
    private bool GetCurrentCameraActiveState()
    {
        if (objectTimelineUI != null)
        {
            return objectTimelineUI.GetCameraActiveState();
        }

        return false; // 默认不激活，确保安全
    }

    // 处理相机激活状态切换
    private void HandleCameraActivation(float time)
    {
        if (!isCamera) return; // 只有相机轨道才需要处理激活状态

        var Clips = clips.OrderBy(c => c.time).ToList();
        if (Clips.Count == 0) return;

        // 找到当前时间点之前最近的关键帧
        TimelineClip currentActiveClip = null;
        for (int i = Clips.Count - 1; i >= 0; i--)
        {
            if (Clips[i].time <= time)
            {
                currentActiveClip = Clips[i];
                break;
            }
        }

        // 如果找到了关键帧，检查其激活状态
        if (currentActiveClip != null)
        {
            bool shouldBeActive = currentActiveClip.isCameraActiveAtTime;
            
            // 避免重复切换：只有当状态真正改变时才执行切换
            if (shouldBeActive != lastCameraActiveState)
            {
                Debug.Log($"[{gameObject.name}] 状态发生变化，执行切换逻辑");
                lastCameraActiveState = shouldBeActive;

                // 检查当前相机的激活状态
                if (CameraManager.Instance != null)
                {
                    var currentSelectedCamera = CameraManager.Instance.GetCurrentSelectedCamera();
                    bool isCurrentlyActive = (currentSelectedCamera != null &&
                                              currentSelectedCamera.gameObject == this.gameObject);

                    // 激活逻辑
                    if (shouldBeActive)
                    {
                        if (!isCurrentlyActive)
                        {
                            // 激活当前相机
                            var cameraController = GetComponentInChildren<CameraController>();
                            if (cameraController != null)
                            {
                                // 确保相机组件是激活的
                                if (cam != null)
                                {
                                    cam.gameObject.SetActive(true);
                                }

                                CameraManager.Instance.SelectCamera(cameraController);
                                Debug.Log($"[{gameObject.name}] 激活相机 (时间: {time:F2}s)");
                            }
                        }
                        else if (!isControlledByMaster)
                        {
                            // 独立模式：如果当前已激活但需要重新激活预览
                            var cameraController = GetComponentInChildren<CameraController>();
                            if (cameraController != null && CameraManager.Instance.previewTexture != null)
                            {
                                cameraController.EnablePreview(CameraManager.Instance.previewTexture);
                                Debug.Log($"[{gameObject.name}] 独立模式重新激活相机预览 (时间: {time:F2}s)");
                            }
                        }
                    }
                    // 禁用逻辑
                    else
                    {
                        if (isControlledByMaster)
                        {
                            if (isCurrentlyActive)
                            {
                                // Master控制模式：切换到其他相机
                                var allCameras = CameraManager.Instance.GetAllCameras();
                                var otherCamera = allCameras.FirstOrDefault(c => c.gameObject != this.gameObject);
                                if (otherCamera != null)
                                {
                                    CameraManager.Instance.SelectCamera(otherCamera);
                                    Debug.Log(
                                        $"[{gameObject.name}] 禁用相机，切换到 {otherCamera.gameObject.name} (时间: {time:F2}s)");
                                }
                            }
                        }
                        else
                        {
                            // 独立模式：无论当前状态如何，都执行禁用
                            var cameraController = GetComponentInChildren<CameraController>();
                            if (cameraController != null)
                            {
                                cameraController.DisablePreview();
                                ClearPreviewTexture(); // 清空预览纹理
                                Debug.Log($"[{gameObject.name}] 独立模式禁用相机预览 (时间: {time:F2}s)");
                            }
                            else if (cam != null)
                            {
                                cam.gameObject.SetActive(false);
                                ClearPreviewTexture(); // 清空预览纹理
                                Debug.Log($"[{gameObject.name}] 独立模式禁用相机组件 (时间: {time:F2}s)");
                            }
                        }
                    }
                }
                else
                {
                    // 没有CameraManager时的处理（独立模式）
                    if (shouldBeActive)
                    {
                        // 激活相机
                        var cameraController = GetComponentInChildren<CameraController>();
                        if (cameraController != null)
                        {
                            // 如果有预览纹理，重新启用预览
                            if (CameraManager.Instance != null && CameraManager.Instance.previewTexture != null)
                            {
                                cameraController.EnablePreview(CameraManager.Instance.previewTexture);
                            }
                            else if (cam != null)
                            {
                                cam.gameObject.SetActive(true);
                            }

                            Debug.Log($"[{gameObject.name}] 独立模式激活相机 (时间: {time:F2}s)");
                        }
                        else if (cam != null)
                        {
                            cam.gameObject.SetActive(true);
                            Debug.Log($"[{gameObject.name}] 独立模式激活相机组件 (时间: {time:F2}s)");
                        }
                    }
                    else
                    {
                        // 禁用相机
                        var cameraController = GetComponentInChildren<CameraController>();
                        if (cameraController != null)
                        {
                            cameraController.DisablePreview();
                            Debug.Log($"[{gameObject.name}] 独立模式禁用相机预览 (时间: {time:F2}s)");
                        }
                        else if (cam != null)
                        {
                            cam.gameObject.SetActive(false);
                            ClearPreviewTexture(); // 清空预览纹理
                            Debug.Log($"[{gameObject.name}] 独立模式禁用相机组件 (时间: {time:F2}s)");
                        }
                    }
                }
            }
        }
    }

    void CheckCameraExists()
    {
        //cam = gameObject.GetComponent<Camera>();
        cam = gameObject.GetComponentInChildren<Camera>();
        if (cam == null)
        {
            isCamera = false;
        }
        else
        {
            isCamera = true;
        }
    }

    void Update()
    {
        if (isControlledByMaster)
        {
            return;
        }

        if (isPlaying && clips.Count > 1)
        {
            currentTime += Time.deltaTime;
            if (currentTime > duration)
            {
                currentTime = duration;
                isPlaying = false; // 播放完毕
                
                // 删除自动添加的0秒关键帧
                DeleteClipsByType(TimelineClip.ClipType.Null);
                
                // 安全地回到第一个关键帧位置
                if (clips.Count > 0)
                {
                    SetTime(clips[0].time);
                }
                else
                {
                    // 如果没有关键帧，回到0秒
                    SetTime(0f);
                }
            }
            else
            {
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
        clip.clipType = TimelineClip.ClipType.ObjectClip;

        if (isCamera)
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

            // 同步UI中的相机激活状态
            if (objectTimelineUI != null && objectTimelineUI.camActiveToggle != null)
            {
                clip.isCameraActiveAtTime = isCameraActive;
            }
            else
            {
                clip.isCameraActiveAtTime = false;
            }
        }
        else
        {
            Debug.Log($"是不是相机{isCamera}");
            clip.fov = 0f;
            clip.focusDistance = 0f;
        }

        /*//TODO：能否用这里来确认不管怎么样开一个相机？或者有CameraManager
        // 记录当前激活的摄像机ID（假设用 InstanceID）
        if (CameraManager.Instance != null && CameraManager.Instance.GetCurrentSelectedCamera() != null)
        {
            clip.activeCameraID = CameraManager.Instance.GetCurrentSelectedCamera().GetInstanceID();
        }
        else
        {
            clip.activeCameraID = -1; // 没有选中摄像机
        }*/

        clips.Add(clip);
        clips = clips.OrderBy(c => c.time).ToList();

        if (clips.Count > 1)
        {
            Debug.Log("计算duration");
            duration = GetDuration();
        }

        MasterTimelineUI.Instance?.RefreshTimelineUI();
        objectTimelineUI.RefreshAll();
    }

    //一般关键帧的添加
    public void AddClip(float time)
    {
        TimelineClip clip = new TimelineClip();
        clip.time = currentTime; //TODO：暂时需自定义
        clip.position = transform.position;
        clip.rotation = transform.rotation;
        clip.scale = transform.localScale;
        clip.clipType = TimelineClip.ClipType.ObjectClip;

        if (isCamera)
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

            // 同步UI中的相机激活状态
            if (objectTimelineUI != null && objectTimelineUI.camActiveToggle != null)
            {
                clip.isCameraActiveAtTime = objectTimelineUI.camActiveToggle.isOn;
            }
            else
            {
                // 如果UI不可用，默认激活
                clip.isCameraActiveAtTime = false;
            }
        }
        else
        {
            clip.fov = 0f;
            clip.focusDistance = 0f;
        }

        /*//TODO：能否用这里来确认不管怎么样开一个相机？或者有CameraManager
        // 记录当前激活的摄像机ID（假设用 InstanceID）
        if (CameraManager.Instance != null && CameraManager.Instance.GetCurrentSelectedCamera() != null)
        {
            clip.activeCameraID = CameraManager.Instance.GetCurrentSelectedCamera().GetInstanceID();
        }
        else
        {
            clip.activeCameraID = -1; // 没有选中摄像机
        }*/

        clips.Add(clip);
        clips = clips.OrderBy(c => c.time).ToList();

        if (clips.Count > 1)
        {
            Debug.Log("计算duration");
            duration = GetDuration();
        }

        MasterTimelineUI.Instance?.RefreshTimelineUI();
        objectTimelineUI.RefreshAll();
    }


    public void AddAnimationClip(string animationName, float animationDuration)
    {
        if (cam != null)
        {
            Debug.LogWarning("这是相机，不可添加动画");
            return;
        }

        TimelineClip preClip = new TimelineClip(); //动画可操作点
        preClip.time = currentTime;
        preClip.position = transform.position;
        preClip.rotation = transform.rotation;
        preClip.scale = transform.localScale;
        preClip.clipType = TimelineClip.ClipType.AnimationClip;
        preClip.animationName = animationName;
        preClip.animationDuration = animationDuration;

        // 检测是否可以添加这个帧
        if (!CanAddAnimationClip(preClip.time, animationDuration))
        {
            Debug.LogError($"[{gameObject.name}] 无法添加动画关键帧: {animationName} - 时间冲突");
            return; // 阻止添加
        }

        clips.Add(preClip);

        TimelineClip autoClip = new TimelineClip(); //动画不看操作点
        autoClip.time = currentTime + animationDuration;
        autoClip.position = transform.position;
        autoClip.rotation = transform.rotation;
        autoClip.scale = transform.localScale;
        autoClip.clipType = TimelineClip.ClipType.AutoAnimationClip;

        clips.Add(autoClip);
        clips = clips.OrderBy(c => c.time).ToList();

        objectTimelineUI.RefreshAll();
        MasterTimelineUI.Instance?.RefreshTimelineUI();

        Debug.Log(
            $"[{gameObject.name}] 成功添加动画关键帧: {animationName} (开始: {currentTime:F2}s, 结束: {currentTime + animationDuration:F2}s)");
    }

    private bool CanAddAnimationClip(float clipTime, float clipDuration)
    {
        var (preAnimClip, nextAnimClip) = GetSurroundingAnimationClips(clipTime);

        // 情况1：前后都有动画帧
        if (preAnimClip != null && nextAnimClip != null)
        {
            // 检查前一个动画是否已经结束
            bool previousAnimationEnded = preAnimClip.time + preAnimClip.animationDuration <= clipTime;
            // 检查新动画是否在下一个动画开始前结束
            bool newAnimationEndsBeforeNext = clipTime + clipDuration <= nextAnimClip.time;

            if (previousAnimationEnded && newAnimationEndsBeforeNext)
            {
                return true;
            }
            else
            {
                Debug.LogWarning(
                    $"[{gameObject.name}] 无法添加动画：时间冲突 (前一个动画结束: {preAnimClip.time + preAnimClip.animationDuration:F2}s, 新动画开始: {clipTime:F2}s, 新动画结束: {clipTime + clipDuration:F2}s, 下一个动画开始: {nextAnimClip.time:F2}s)");
                return false;
            }
        }
        // 情况2：只有前一个动画帧
        else if (nextAnimClip == null && preAnimClip != null)
        {
            // 检查前一个动画是否已经结束
            bool previousAnimationEnded = preAnimClip.time + preAnimClip.animationDuration <= clipTime;

            if (previousAnimationEnded)
            {
                return true;
            }
            else
            {
                Debug.LogWarning(
                    $"[{gameObject.name}] 无法添加动画：前一个动画还未结束 (前一个动画结束: {preAnimClip.time + preAnimClip.animationDuration:F2}s, 新动画开始: {clipTime:F2}s)");
                return false;
            }
        }
        // 情况3：只有下一个动画帧
        else if (preAnimClip == null && nextAnimClip != null)
        {
            // 检查新动画是否在下一个动画开始前结束
            bool newAnimationEndsBeforeNext = clipTime + clipDuration <= nextAnimClip.time;

            if (newAnimationEndsBeforeNext)
            {
                return true;
            }
            else
            {
                Debug.LogWarning(
                    $"[{gameObject.name}] 无法添加动画：新动画与下一个动画冲突 (新动画结束: {clipTime + clipDuration:F2}s, 下一个动画开始: {nextAnimClip.time:F2}s)");
                return false;
            }
        }
        // 情况4：没有任何动画帧
        else
        {
            return true;
        }
    }

    [ContextMenu("添加动画关键帧")]
    public void AddAnimationClip()
    {
        AddAnimationClip(animName, animDuration);
    }


    public bool HasClipAtTime(float time)
    {
        return clips.Any(c => Mathf.Approximately(c.time, time));
    }

    public TimelineClip GetClipAtTime(float time)
    {
        return clips.FirstOrDefault(c => Mathf.Approximately(c.time, time));
    }


    public List<float> GetAllClipTimes()
    {
        return clips.Select(c => c.time).ToList();
    }

    public (TimelineClip previous, TimelineClip next) GetSurroundingAnimationClips(float time)
    {
        TimelineClip previous = null;
        TimelineClip next = null;

        // 按时间排序，只有动画操作帧
        var sortedAnimationClips = clips
            .Where(c => c.clipType == TimelineClip.ClipType.AnimationClip)
            .OrderBy(c => c.time)
            .ToList();

        for (int i = 0; i < sortedAnimationClips.Count; i++)
        {
            if (sortedAnimationClips[i].time < time)
            {
                previous = sortedAnimationClips[i];
            }
            else if (sortedAnimationClips[i].time > time)
            {
                next = sortedAnimationClips[i];
                break;
            }
        }

        return (previous, next);
    }


    void InitializeTimelineUI()
    {
        Debug.Log($"[{gameObject.name}] 开始初始化UI");

        // 如果没有设置UI，尝试自动创建
        if (objectTimelineUI == null && (objectTimelineUIPrefab != null || cameraTimelineUIPrefab != null))
        {
            Debug.Log($"[{gameObject.name}] 创建新的UI实例");
            // 实例化prefab
            GameObject uiInstance = null;
            if (this.gameObject.GetComponentInChildren<Camera>() != null && cameraTimelineUIPrefab != null)
            {
                //这是摄像机
                Debug.Log("实例化相机的操作Canvas");
                uiInstance = Instantiate(cameraTimelineUIPrefab);
            }
            else
            {
                uiInstance = Instantiate(objectTimelineUIPrefab);
            }

            uiInstance.name = $"{gameObject.name} Canvas";
            objectTimelineUI = uiInstance.GetComponent<ObjectTimelineUI>();
            // 初始化UI并绑定到当前轨道
            objectTimelineUI.Initialize(this);
            // 初始化后默认隐藏UI，等待用户选择时显示
            hideUI();
        }
        else if (objectTimelineUI != null)
        {
            Debug.Log($"[{gameObject.name}] 使用现有UI实例");
            // 如果已经设置了UI，直接初始化
            objectTimelineUI.Initialize(this);
            // 初始化后默认隐藏UI，等待用户选择时显示
            //hideUI();
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 无法初始化UI：objectTimelineUI为空且objectTimelineUIPrefab为空");
        }
    }

    [ContextMenu("播放")]
    public void Play()
    {
        isPlaying = true;
        currentTime = 0f;

        if (!HasClipAtTime(currentTime))
        {
            TimelineClip zeroClip = new TimelineClip();
            zeroClip.time = currentTime;
            zeroClip.position = startPosition;
            zeroClip.rotation = startRotation;
            zeroClip.scale = startScale;
            zeroClip.clipType = TimelineClip.ClipType.Null;

            // 相机0秒关键帧的处理
            if (isCamera)
            {
                // 根据UI状态或默认设置相机激活状态
                zeroClip.isCameraActiveAtTime = false;

                Debug.Log($"[{gameObject.name}] 创建0秒关键帧，相机激活状态: {zeroClip.isCameraActiveAtTime}");
            }

            clips.Add(zeroClip);
        }

        clips = clips.OrderBy(c => c.time).ToList();
        duration = GetDuration();

        // 重置相机激活状态，确保在播放开始时正确处理
        lastCameraActiveState = false;

        // 强制处理0秒时的相机激活状态
        if (isCamera)
        {
            var zeroClip = clips.FirstOrDefault(c => c.time == 0f);
            if (zeroClip != null)
            {
                // 强制设置一个不同的状态，确保会执行切换逻辑
                lastCameraActiveState = !zeroClip.isCameraActiveAtTime;
                Debug.Log($"[{gameObject.name}] 强制设置lastCameraActiveState为 {lastCameraActiveState}，确保0秒关键帧生效");
            }
        }

        Debug.Log($"[{gameObject.name}] 开始播放时间线，已重置动画状态");
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
        var nonAutoAnimationClips = clips.Where(c => c.clipType != TimelineClip.ClipType.AutoAnimationClip)
            .OrderBy(c => c.time).ToList();
        if (nonAutoAnimationClips.Count() == 0) return;

        // 处理相机激活状态切换（必须在所有其他逻辑之前执行）
        HandleCameraActivation(time);

        // 如果只有一个0秒关键帧且不是动画，只处理相机激活，不处理其他属性
        if (nonAutoAnimationClips.Count() == 1 &&
            nonAutoAnimationClips.First().clipType != TimelineClip.ClipType.AnimationClip &&
            nonAutoAnimationClips.First().time == 0f)
        {
            // 只处理相机激活状态，不处理位置等属性
            return;
        }

        //Camera cam = GetComponentInChildren<Camera>();
        // 检查并触发动画关键帧

        if (time < nonAutoAnimationClips.First().time && (isPlaying || isControlledByMaster))
        {
            transform.position = new Vector3(99999, 99999, 99999);
            // 检查并触发动画关键帧
            CheckAndTriggerAnimationClip(time);
            return;
        }
        else if (time < nonAutoAnimationClips.First().time && !isPlaying)
        {
            var first = nonAutoAnimationClips.First();
            transform.position = first.position;
            transform.rotation = first.rotation;
            transform.localScale = first.scale;
            if (cam != null)
            {
                cam.fieldOfView = first.fov;
                if (dof != null)
                    dof.focusDistance.value = first.focusDistance;
            }

            return;
        }

        // 2. time晚于最后一个关键帧，或isControlledByMaster时超出范围
        if (time >= nonAutoAnimationClips.Last().time ||
            (isControlledByMaster && time >= nonAutoAnimationClips.Last().time))
        {
            var last = nonAutoAnimationClips.Last();
            transform.position = last.position;
            transform.rotation = last.rotation;
            transform.localScale = last.scale;
            if (cam != null)
            {
                cam.fieldOfView = last.fov;
                if (dof != null)
                    dof.focusDistance.value = last.focusDistance;
            }

            // 检查并触发动画关键帧
            CheckAndTriggerAnimationClip(time);
            return;
        }

        // 3. 在关键帧区间内，找到前后帧（使用nonAutoAnimationClips）
        TimelineClip prev = null, next = null;
        for (int i = 0; i < nonAutoAnimationClips.Count() - 1; i++)
        {
            if (nonAutoAnimationClips[i].time <= time && nonAutoAnimationClips[i + 1].time >= time)
            {
                prev = nonAutoAnimationClips[i];
                next = nonAutoAnimationClips[i + 1];
                break;
            }
        }
        // 找到当前时间点之前最近且激活摄像机的关键帧
        /*TimelineClip activeCamClip = null;
        for (int i = nonAutoAnimationClips.Count - 1; i >= 0; i--)
        {
            if (nonAutoAnimationClips[i].time <= time && nonAutoAnimationClips[i].isCameraActiveAtTime)
            {
                activeCamClip = nonAutoAnimationClips[i];
                break;
            }
        }

        if (activeCamClip != null)
        {
            int activeID = activeCamClip.activeCameraID;
            CameraController toActivate = null;

            if (CameraManager.Instance != null)
            {
                foreach (var camCtrl in CameraManager.Instance.GetAllCameras())
                {
                    if (camCtrl.GetInstanceID() == activeID)
                    {
                        toActivate = camCtrl;
                        break;
                    }
                }

                if (toActivate != null)
                {
                    CameraManager.Instance.SelectCamera(toActivate);
                }
            }
        }*/

        // 检查并触发动画关键帧
        CheckAndTriggerAnimationClip(time);
        if (prev == null || next == null) return; // 理论上不会发生

        float delta = next.time - prev.time;
        float t = 0f;
        if (Mathf.Approximately(delta, 0f))
            t = 0f;
        else
            t = (time - prev.time) / delta;

        transform.position = Vector3.Lerp(prev.position, next.position, t);
        transform.rotation = Quaternion.Slerp(prev.rotation, next.rotation, t);
        transform.localScale = Vector3.Lerp(prev.scale, next.scale, t);

        //TODO: 摄像机
        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(prev.fov, next.fov, t);
            if (dof != null)
                dof.focusDistance.value = Mathf.Lerp(prev.focusDistance, next.focusDistance, t);
        }
    }


    public TimelineClip FindNearestNonAutoAnimationClip(float time)
    {
        if (clips.Count == 0) return null;

        TimelineClip nearest = null;
        float minDistance = float.MaxValue;

        foreach (var clip in clips)
        {
            // 排除动画补帧类型的关键帧
            if (clip.clipType == TimelineClip.ClipType.AutoAnimationClip)
            {
                continue;
            }

            float distance = Mathf.Abs(clip.time - time);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = clip;
            }
        }

        return nearest;
    }

    public float GetDuration()
    {
        if (clips.Count == 0) return 0f;
        float maxTime = clips[clips.Count - 1].time;
        return maxTime;
    }

    public void SetTime(float time)
    {
        currentTime = time;
        ApplyClipAtTime(time);
    }

    public void showUI()
    {
        Debug.Log("显示UI预制体");
        //objectTimelineUI.gameObject.SetActive(true);
        //objectTimelineUI.ShowPanel(this);

        if (objectTimelineUI != null)
        {
            objectTimelineUI.gameObject.SetActive(true);
            objectTimelineUI.ShowPanel(this);
        }
        else
        {
            Debug.LogWarning("objectTimelineUI 为空，无法显示UI");
        }
    }

    public void hideUI()
    {
        Debug.Log("隐藏UI实例");
        objectTimelineUI.HidePanel();
    }

    public bool DeleteClipAtTime(float time)
    {
        var clipToDelete = clips.FirstOrDefault(c => Mathf.Approximately(c.time, time));
        if (clipToDelete == null)
        {
            Debug.LogWarning($"时间 {time:F2}s 没有找到关键帧");
            return false;
        }
        else if (clipToDelete.clipType == TimelineClip.ClipType.AnimationClip ||
                 clipToDelete.clipType == TimelineClip.ClipType.AutoAnimationClip)
        {
            if (clipToDelete.clipType == TimelineClip.ClipType.AnimationClip)
            {
                var autoAnimationClipToDelete = clips.FirstOrDefault(c =>
                    Mathf.Approximately(c.time, time + clipToDelete.animationDuration));
                if (autoAnimationClipToDelete != null)
                {
                    Debug.LogWarning("没有找到对应的动画填补帧");
                    return false;
                }

                clips.Remove(autoAnimationClipToDelete);
                clips.Remove(clipToDelete);
                UpdateTimelineAfterClipChange();
                Debug.Log($"已删除时间 {time:F2}s 的关键帧");
                return true;
            }
            else if (clipToDelete.clipType == TimelineClip.ClipType.AutoAnimationClip)
            {
                Debug.LogWarning("动画补位帧不可操作");
            }
        }

        clips.Remove(clipToDelete);
        UpdateTimelineAfterClipChange();
        Debug.Log($"已删除时间 {time:F2}s 的关键帧");
        return true;
    }

    public int DeleteClipsByType(TimelineClip.ClipType clipType)
    {
        int deletedCount = clips.RemoveAll(c => c.clipType == clipType);
        if (deletedCount > 0)
        {
            UpdateTimelineAfterClipChange();
            Debug.Log($"已删除 {deletedCount} 个类型为 {clipType} 的关键帧");
        }

        return deletedCount;
    }

    public void DeleteAllClips()
    {
        int clipCount = clips.Count;
        clips.Clear();
        UpdateTimelineAfterClipChange();
        Debug.Log($"已删除所有 {clipCount} 个关键帧");
    }

    private void UpdateTimelineAfterClipChange()
    {
        if (clips.Count > 1)
        {
            duration = GetDuration();
        }
        else if (clips.Count == 1)
        {
            duration = clips[0].time;
        }
        else
        {
            duration = 0f;
        }

        clips = clips.OrderBy(c => c.time).ToList();
        MasterTimelineUI.Instance?.RefreshTimelineUI();
        objectTimelineUI.RefreshAll();
    }


    private void CheckAndTriggerAnimationClip(float currentTime)
    {
        // 查找当前时间点的动画关键帧
        var animationClip = clips.FirstOrDefault(c =>
            c.clipType == TimelineClip.ClipType.AnimationClip &&
            currentTime >= c.time && currentTime < c.time + 0.1f); // 允许0.1秒的误差

        var autoAnimationClip = clips.FirstOrDefault(c =>
            c.clipType == TimelineClip.ClipType.AutoAnimationClip &&
            currentTime >= c.time && currentTime < c.time + 0.1f); // 允许0.1秒的误差

        // 处理AnimationClip（动画开始）- 防止重复播放
        if (animationClip != null && !isAnimPlaying)
        {
            isAnimPlaying = true; // 标记为已播放
            CharacterActionController characterActionController = gameObject.GetComponent<CharacterActionController>();
            //characterActionController.PlayAction(animationClip.animationName,false);
            Animator animator = gameObject.GetComponent<Animator>();
            if (characterActionController == null)
            {
                Debug.Log("Animator报空");
                return;
            }
            else
            {
                animator.Play(animationClip.animationName);
                animator.SetFloat("Speed", 1f); // 确保有一个控制速度的参数
                StartCoroutine(StopAnimationAfterTime(duration));
            }
        }

        //TODO:因为ClipType的原因所以不能进入
        // 处理AutoAnimationClip（动画结束）
        if (autoAnimationClip != null && isAnimPlaying)
        {
            isAnimPlaying = false;
            Debug.Log($"[{gameObject.name}] 停止播放动画 (时间: {currentTime:F2}s, 目标时间: {autoAnimationClip.time:F2}s)");
        }
    }

    IEnumerator StopAnimationAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetFloat("Speed", 0f); // 停止播放
        animator.Play("T-Pose");
        isAnimPlaying = false;
        Debug.Log($"[{gameObject.name}] 停止播放动画 (时间: {currentTime:F2}s");
    }


    [ContextMenu("删除当前时间关键帧")]
    public void DeleteCurrentTimeClip()
    {
        bool success = DeleteClipAtTime(currentTime);
        if (success)
        {
            Debug.Log($"[{gameObject.name}] 成功删除当前时间 {currentTime:F2}s 的关键帧");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 当前时间 {currentTime:F2}s 没有关键帧可删除");
        }
    }

    [ContextMenu("删除所有关键帧")]
    public void DeleteAllClipsContextMenu()
    {
        DeleteAllClips();
        Debug.Log($"[{gameObject.name}] 已清空所有关键帧");
    }

    [ContextMenu("删除动画关键帧")]
    public void DeleteAnimationClips()
    {
        int deletedCount = DeleteClipsByType(TimelineClip.ClipType.AnimationClip);
        Debug.Log($"[{gameObject.name}] 删除了 {deletedCount} 个动画关键帧");
    }

    [ContextMenu("显示所有关键帧时间")]
    public void ShowAllClipTimes()
    {
        List<float> times = GetAllClipTimes();
        if (times.Count == 0)
        {
            Debug.Log($"[{gameObject.name}] 没有关键帧");
            return;
        }

        string timeList = string.Join(", ", times.Select(t => $"{t:F2}s"));
        Debug.Log($"[{gameObject.name}] 所有关键帧时间: {timeList}");
    }

    [ContextMenu("验证时间计算逻辑")]
    public void ValidateTimeCalculation()
    {
        if (clips.Count == 0)
        {
            Debug.Log("时间线为空，无法验证");
            return;
        }

        // 验证clips是否按时间排序
        bool isSorted = true;
        for (int i = 1; i < clips.Count; i++)
        {
            if (clips[i].time < clips[i - 1].time)
            {
                isSorted = false;
                break;
            }
        }

        Debug.Log($"clips是否按时间排序: {isSorted}");

        // 获取最大时间点
        float maxTimeFromLast = clips[clips.Count - 1].time;
        float maxTimeFromLoop = 0f;

        foreach (var clip in clips)
        {
            if (clip.time > maxTimeFromLoop)
                maxTimeFromLoop = clip.time;
        }

        Debug.Log($"最后一个关键帧时间: {maxTimeFromLast:F2}s");
        Debug.Log($"循环计算最大时间: {maxTimeFromLoop:F2}s");
        Debug.Log($"两种方法结果是否一致: {Mathf.Approximately(maxTimeFromLast, maxTimeFromLoop)}");

        // 检查动画关键帧
        int animationClipCount = 0;
        foreach (var clip in clips)
        {
            if (clip.clipType == TimelineClip.ClipType.AnimationClip)
            {
                animationClipCount++;
                Debug.Log($"发现动画关键帧: 时间={clip.time:F2}s");
            }
        }

        Debug.Log($"动画关键帧数量: {animationClipCount}");

        // 显示当前duration
        Debug.Log($"当前duration: {duration:F2}s");
        //Debug.Log($"GetMaxTime(): {GetMaxTime():F2}s");
        //Debug.Log($"GetDurationWithAnimation(): {GetDurationWithAnimation():F2}s");
    }

    // 手动检查并更新相机激活状态
    public void CheckCameraActivationState()
    {
        HandleCameraActivation(currentTime);
    }

    // 清空预览纹理
    private void ClearPreviewTexture()
    {
        if (CameraManager.Instance != null && CameraManager.Instance.previewTexture != null)
        {
            RenderTexture.active = CameraManager.Instance.previewTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;
            Debug.Log($"[{gameObject.name}] 清空预览纹理");
        }
    }

    public CameraController GetExpectedActiveCameraControllerAtTime(float time)
    {
        // 找到当前时间点最近的关键帧
        var Clips = clips.OrderBy(c => c.time).ToList();
        TimelineClip currentActiveClip = null;
        for (int i = Clips.Count - 1; i >= 0; i--)
        {
            if (Clips[i].time <= time)
            {
                currentActiveClip = Clips[i];
                break;
            }
        }

        if (currentActiveClip != null && currentActiveClip.isCameraActiveAtTime)
        {
            return GetComponentInChildren<CameraController>();
        }

        return null;
    }

    [ContextMenu("测试用例: 播放结束后回到第一帧")]
    public void TestPlaybackEndReturnToFirstFrame()
    {
        Debug.Log("=== 测试用例: 播放结束后回到第一帧 ===");
        
        // 清空现有关键帧
        DeleteAllClips();
        
        // 添加多个关键帧
        currentTime = 0f;
        AddClip();
        currentTime = 2f;
        AddClip();
        currentTime = 4f;
        AddClip();
        
        Debug.Log($"创建测试关键帧: 0s -> 2s -> 4s (总时长: {duration}s)");
        Debug.Log($"初始位置: {transform.position}");
        
        // 记录第一个关键帧的位置
        var firstClip = clips[0];
        Vector3 firstPosition = firstClip.position;
        
        // 移动到中间位置
        SetTime(2f);
        Debug.Log($"移动到2s位置: {transform.position}");
        
        // 开始播放
        Play();
        Debug.Log($"开始播放: isPlaying={isPlaying}, currentTime={currentTime}");
        
        // 模拟播放结束（手动设置时间到结束）
        currentTime = duration;
        isPlaying = false;
        
        // 删除自动添加的0秒关键帧
        DeleteClipsByType(TimelineClip.ClipType.Null);
        
        // 回到第一个关键帧
        if (clips.Count > 0)
        {
            SetTime(clips[0].time);
            Debug.Log($"播放结束后回到第一帧: {transform.position}");
            Debug.Log($"第一帧原始位置: {firstPosition}");
            
            // 检查是否回到了正确位置
            if (Vector3.Distance(transform.position, firstPosition) < 0.1f)
            {
                Debug.Log("✓ 成功回到第一帧位置");
            }
            else
            {
                Debug.LogError("✗ 没有回到第一帧位置");
            }
        }
        else
        {
            Debug.LogWarning("没有关键帧，回到0秒");
            SetTime(0f);
        }
    }
}