using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CharacterActionController : MonoBehaviour
{
    public Animator animator;
    public List<CharacterAction> actionSequence = new List<CharacterAction>();

    private int currentIndex = 0;
    private float timer = 0f;
    private bool isPlaying = false;
    // 添加一个 public 只读属性
    public bool IsPlaying => isPlaying;


    private bool isPreviewMode = false;
    private float previewTimer = 0f;
    private float previewMaxDuration = 5f;

    public bool isPlacedCharacter = false;

    private void Update()
    {
        if (isPreviewMode)
        {
            previewTimer += Time.deltaTime;
            if (previewTimer >= previewMaxDuration)
            {
                StopPreview();
            }
            return;
        }

        if (!isPlaying || actionSequence.Count == 0 || currentIndex >= actionSequence.Count) return;
        Debug.Log($"播放第 {currentIndex} 个动作 {actionSequence[currentIndex].actionName}，已计时 {timer} / {actionSequence[currentIndex].duration}");
        timer += Time.deltaTime;

        if (timer >= actionSequence[currentIndex].duration)
        {
            currentIndex++;
            if (currentIndex >= actionSequence.Count)
            {
                isPlaying = false;
                /*animator.Play("T-Pose", 0, 0f);*/
                animator.CrossFade("T-Pose", 0f, 0, 0f);
                Debug.Log("动作序列播放完毕，切回 T-Pose");
                return;
            }

            PlayAction(actionSequence[currentIndex]);
        }
    }

    
    public void PlayAction(CharacterAction action, bool preview = false)
    {
        if (animator == null)
        {
            Debug.LogError("Animator 未赋值！");
            return;
        }

        isPreviewMode = preview;
        if (preview) previewTimer = 0f;
        Debug.Log($"播放动作: {action.actionName}，时长: {action.duration}, 预览模式: { preview}");
        /*animator.Play(action.actionName);*/
        /* animator.Play(action.actionName, 0, 0f);  // 从头播放当前动作*/
        // CrossFade到动作，过渡时间设为0，确保从头开始
        animator.CrossFade(action.actionName, 0f, 0, 0f);

        animator.Update(0f);                      // 强制刷新动画状态

        timer = 0f;
    }

    public void StopPreview()
    {
        /*isPreviewMode = false;
        animator.Play("T-Pose", 0, 0f);  // 切回Idle动画（记得有Idle状态）*/
        if (isPreviewMode)
        {
            Debug.Log("预览结束，切换回 Idle 动作");
            isPreviewMode = false;
            previewTimer = 0f;
            animator.Play("T-Pose", 0, 0f);
        }
    }


    public void PlaySequence()
    {
        if (actionSequence.Count == 0)
        {
            Debug.LogWarning("动作序列为空，无法播放！");
            return;
        }

        currentIndex = 0;
        timer = 0f;
        isPlaying = true;
        isPreviewMode = false;
        Debug.Log($"开始播放动作序列，共 {actionSequence.Count} 个动作");
       /* PlayAction(actionSequence[currentIndex]);*/
        // 立即CrossFade播放第一个动作
        animator.CrossFade(actionSequence[currentIndex].actionName, 0f, 0, 0f);
    }

    public void PauseSequence()
    {
        isPlaying = false;
    }

    public void ResumeSequence()
    {
        isPlaying = true;
    }

    public void StopSequence()
    {
        isPlaying = false;
        currentIndex = 0;
        animator.Play("T-Pose", 0, 0f);
    }

    public void AddAction(CharacterAction action)
    {
        actionSequence.Add(action);
        Debug.Log($"添加了动作: {action.actionName}，当前序列长度: {actionSequence.Count}");
        TimelineTrack track=GetComponent<TimelineTrack>();
        track.AddAnimationClip(action.duration);
    }

    public void ClearActions()
    {
        actionSequence.Clear();
        currentIndex = 0;        // ✅ 重置当前索引
        timer = 0f;              // ✅ 可选：防止残留时间触发
        isPlaying = false;       // ✅ 可选：确保不再尝试播放
        isPreviewMode = false;
    }

    IEnumerator PlayActionWithDelay(CharacterAction action)
    {
        animator.Play(action.actionName, 0, 0f);
        yield return null;  // 等一帧
        animator.Update(0f);
        timer = 0f;
    }

}
