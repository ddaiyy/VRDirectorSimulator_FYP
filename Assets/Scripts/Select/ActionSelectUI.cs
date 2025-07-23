using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionSelectUI : MonoBehaviour
{
    public CharacterAction actionData;
    public Button viewButton;
    public Button selectButton;

    public Slider durationSlider;
    public TMP_Text durationText;

    private void Start()
    {
        // 设置滑动条范围（初始为 0）
        durationSlider.minValue = 0f;
        durationSlider.maxValue = 10f;
        durationSlider.value = 0f; // 初始值为 0，代表未选择

        durationSlider.onValueChanged.AddListener(OnSliderValueChanged);

        viewButton.onClick.AddListener(PreviewAction);
        selectButton.onClick.AddListener(SelectAction);

        OnSliderValueChanged(durationSlider.value); // 初始化 UI 状态（让 select 灰掉）
    }

    void OnSliderValueChanged(float value)
    {
        UpdateDurationText(value);

        var target = SelectedCharacterManager.CurrentSelectedCharacter;
        // 如果没有选中角色，或角色未放置（isPlacedCharacter为false），始终禁用select按钮
        if (target == null || !target.isPlacedCharacter)
        {
            selectButton.interactable = false;
        }
        else
        {
            // 放置过的角色，根据滑动条值来决定是否启用select按钮
            selectButton.interactable = value > 0f;
        }
    }


    void UpdateDurationText(float value)
    {
        durationText.text = $"Duration: {value:F1} s";
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
        if (target != null && target.isPlacedCharacter)
        {
            float userDuration = durationSlider.value;
            actionData.duration = userDuration;

            target.StopPreview();
            target.AddAction(actionData);
        }
        else
        {
            Debug.LogWarning("当前选中角色不能添加动作！");
        }
    }

    public void ResetSlider()
    {
        durationSlider.value = 0f;
    }


    [ContextMenu("测试 PreviewAction()")]
    void TestPreviewAction()
    {
        PreviewAction();
    }

    [ContextMenu("测试 SelectAction()")]
    void TestSelectAction()
    {
        SelectAction();
    }
}
