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
            Debug.LogError("Animator δ��ֵ��");
            return;
        }

        Debug.Log($"���Ŷ���: {action.actionName}��ʱ��: {action.duration}");
        /*animator.Play(action.actionName);*/
        animator.Play(action.actionName, 0, 0f);  // ��ͷ���ŵ�ǰ����
        animator.Update(0f);                      // ǿ��ˢ�¶���״̬

        timer = 0f;
    }

  
    public void PlaySequence()
    {
        if (actionSequence.Count == 0)
        {
            Debug.LogWarning("��������Ϊ�գ��޷����ţ�");
            return;
        }

        currentIndex = 0;
        isPlaying = true;
        Debug.Log($"��ʼ���Ŷ������У��� {actionSequence.Count} ������");
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
        Debug.Log($"����˶���: {action.actionName}����ǰ���г���: {actionSequence.Count}");
    }

    public void ClearActions()
    {
        actionSequence.Clear();
    }
}
