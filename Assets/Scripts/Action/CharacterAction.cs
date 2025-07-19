using UnityEngine;

[System.Serializable]
public class CharacterAction
{
    public string actionName; // 必须与 Animator 中的状态名一致
    public AnimationClip animationClip; // 动画片段（可选）
    public float duration = 2f; // 用户自定义播放时间
}
