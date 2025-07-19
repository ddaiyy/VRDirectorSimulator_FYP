using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionSelectUI : MonoBehaviour
{
    public CharacterAction actionData;
    public Button viewButton;
    public Button selectButton;

    public TMP_InputField durationInputField; // 👈 用户设置时间的输入框（例如 2.5）

    private void Start()
    {
        viewButton.onClick.AddListener(PreviewAction);
        selectButton.onClick.AddListener(SelectAction);
    }

    void PreviewAction()
    {
        var target = SelectedCharacterManager.CurrentSelectedCharacter;
        if (target != null)
        {
            target.StopPreview();
            target.PlayAction(actionData, true);
        }
        else
        {
            Debug.LogWarning("尚未选择角色！");
        }
    }

    void SelectAction()
    {
        var target = SelectedCharacterManager.CurrentSelectedCharacter;
        if (target != null)
        {
            string inputText = durationInputField.text;
            Debug.Log($"用户输入的持续时间文本是: {inputText}");
            // ✅ 获取用户输入时间（如为空就用默认）
            if (float.TryParse(durationInputField.text, out float userDuration))
            {
                Debug.Log($"解析成功，用户设置的动作持续时间是: {userDuration} 秒");
                actionData.duration = userDuration;
            }
            else
            {
                Debug.LogWarning("用户输入的持续时间格式不正确，未修改duration");
            }
            target.StopPreview(); // 👉 停止预览，避免状态干扰
            target.AddAction(actionData);
        }
        else
        {
            Debug.LogWarning("尚未选择角色！");
        }


    }

    // 👇 Inspector 中右键组件 → 点击此项可直接测试 Preview
    [ContextMenu("测试 PreviewAction()")]
    void TestPreviewAction()
    {
        PreviewAction();
    }

    // 👇 Inspector 中右键组件 → 点击此项可直接测试 Select
    [ContextMenu("测试 SelectAction()")]
    void TestSelectAction()
    {
        SelectAction();
    }
}
