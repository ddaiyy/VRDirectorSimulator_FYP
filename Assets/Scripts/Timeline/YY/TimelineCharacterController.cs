using UnityEngine;

public class TimelineCharacterController : MonoBehaviour
{
    public CharacterActionController characterController;

    public CharacterAction[] availableActions;  // 从 Inspector 设置
    public string currentActionName;
    public float actionStartTime;
    public float actionDuration;
    private bool isActionPlaying = false;

    public void PlayActionByName(string actionName)
    {
        CharacterAction matchedAction = null;
        foreach (var action in availableActions)
        {
            if (action.actionName == actionName)
            {
                matchedAction = action;
                break;
            }
        }

        if (matchedAction != null)
        {
            characterController.PlayAction(matchedAction);
            currentActionName = matchedAction.actionName;
            actionStartTime = Time.time;
            isActionPlaying = true;

            Debug.Log($"▶️ {GetCharacterName()} 开始动作 {matchedAction.actionName}，时间：{actionStartTime:F2}s");
        }
        else
        {
            Debug.LogWarning($"❌ 未找到名为 {actionName} 的动作！");
        }
    }

    public void StopAction()
    {
        if (isActionPlaying)
        {
            actionDuration = Time.time - actionStartTime;
            isActionPlaying = false;

            Debug.Log($"⏹️ {GetCharacterName()} 停止动作 {currentActionName}，持续时间：{actionDuration:F2}s");
        }
    }

    public string GetCurrentActionName() => currentActionName;
    public string GetCharacterName() => characterController.gameObject.name;
    public float GetCurrentActionStartTime() => actionStartTime;
    public float GetCurrentActionDuration() => actionDuration;
    public bool IsActionPlaying() => isActionPlaying;
}
