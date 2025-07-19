using UnityEngine;
using UnityEngine.UI;

public class ActionSelectUI : MonoBehaviour
{
    public CharacterAction actionData;
    public Button viewButton;
    public Button selectButton;

    private void Start()
    {
        viewButton.onClick.AddListener(PreviewAction);
        selectButton.onClick.AddListener(SelectAction);
    }

    void PreviewAction()
    {
        var target = SelectedCharacterManager.CurrentSelectedCharacter;
        if (target != null)
            target.PlayAction(actionData);
        else
            Debug.LogWarning("尚未选择角色！");
    }

    void SelectAction()
    {
        var target = SelectedCharacterManager.CurrentSelectedCharacter;
        if (target != null)
            target.AddAction(actionData);
        else
            Debug.LogWarning("尚未选择角色！");
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
