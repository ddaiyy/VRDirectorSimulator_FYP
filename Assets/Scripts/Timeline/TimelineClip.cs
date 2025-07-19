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
    public bool isActive = true;
} 