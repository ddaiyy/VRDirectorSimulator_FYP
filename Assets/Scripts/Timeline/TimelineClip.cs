using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimelineClip
{
    //关键帧时间
    public float time;
    
    //位置
    public Vector3 position;
    
    //旋转
    public Quaternion rotation;
    
    //摄像机视野角度
    public float fov = 60f;
    
    //缩放
    public Vector3 scale;
    
    //是否激活
    //public bool isActive = true;
    public bool isCameraActiveAtTime = false;
    // 记录景深焦距
    public float focusDistance = 5f;

    //记录摄像机的ID
    //public int activeCameraID;
    
    //
    public enum ClipType
    {
        Null,
        ObjectClip, 
        CameraClip,
        AnimationClip,
        AutoAnimationClip,//不可以删除除非对应的前面的Clip删除了
    }
    public ClipType clipType= ClipType.Null;

    public string animationName;
    public float animationDuration;
    //public bool animationPlayed=false;
} 