using System.Collections.Generic;
using UnityEngine;

public class CharacterActionController : MonoBehaviour
{
    public Animator animator;
    public List<CharacterAction> actionSequence = new List<CharacterAction>();

    private int currentIndex = 0;
    private float timer = 0f;
    private bool isPlaying = false;

    private void Update()
    {
        if (!isPlaying || actionSequence.Count == 0) return;

        timer += Time.deltaTime;
        if (timer >= actionSequence[currentIndex].duration)
        {
            currentIndex++;
            if (currentIndex >= actionSequence.Count)
            {
                isPlaying = false;
                return;
            }

            PlayAction(actionSequence[currentIndex]);
        }
    }

    
    public void PlayAction(CharacterAction action)
    {
        if (animator == null)
        {
            Debug.LogError("Animator 未赋值！");
            return;
        }

        Debug.Log($"播放动作: {action.actionName}，时长: {action.duration}");
        /*animator.Play(action.actionName);*/
        animator.Play(action.actionName, 0, 0f);  // 从头播放当前动作
        animator.Update(0f);                      // 强制刷新动画状态

        timer = 0f;
    }

  
    public void PlaySequence()
    {
        if (actionSequence.Count == 0)
        {
            Debug.LogWarning("动作序列为空，无法播放！");
            return;
        }

        currentIndex = 0;
        isPlaying = true;
        Debug.Log($"开始播放动作序列，共 {actionSequence.Count} 个动作");
        PlayAction(actionSequence[currentIndex]);
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
    }

    public void AddAction(CharacterAction action)
    {
        actionSequence.Add(action);
        Debug.Log($"添加了动作: {action.actionName}，当前序列长度: {actionSequence.Count}");
    }

    public void ClearActions()
    {
        actionSequence.Clear();
    }
}
