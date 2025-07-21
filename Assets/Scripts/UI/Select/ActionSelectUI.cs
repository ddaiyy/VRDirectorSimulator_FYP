/*using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionSelectUI : MonoBehaviour
{
    public CharacterAction actionData;
    public Button viewButton;
    public Button selectButton;
    public TMP_InputField durationInputField;

    private void OnEnable()
    {
        UpdateUIState();
    }

    private void Start()
    {
        viewButton.onClick.AddListener(PreviewAction);
        selectButton.onClick.AddListener(SelectAction);
        durationInputField.onValueChanged.AddListener(CheckDurationValid); // 实时检查
    }

    void UpdateUIState()
    {
        var target = SelectedCharacterManager.CurrentSelectedCharacter;

        if (target != null && target.isPlacedCharacter) // ✅ 被抓取的角色
        {
            durationInputField.gameObject.SetActive(true);
            durationInputField.text = "";
            selectButton.interactable = false;
        }
        else // ❌ 放在平面上的角色
        {
            durationInputField.gameObject.SetActive(false);
            selectButton.interactable = false;
        }
    }

    void CheckDurationValid(string input)
    {
        var target = SelectedCharacterManager.CurrentSelectedCharacter;

        if (target != null && target.isPlacedCharacter)
        {
            selectButton.interactable = float.TryParse(input, out float result) && result > 0f;
        }
        else
        {
            selectButton.interactable = false;
        }
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
            string inputText = durationInputField.text;
            if (float.TryParse(inputText, out float userDuration))
            {
                actionData.duration = userDuration;
            }
            else
            {
                Debug.LogWarning("持续时间格式不正确");
            }

            target.StopPreview();
            target.AddAction(actionData);
        }
        else
        {
            Debug.LogWarning("当前选中角色不能添加动作！");
        }
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
*/


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
        // 初始化滑动条范围和值
        durationSlider.minValue = 0.5f;
        durationSlider.maxValue = 10f;
        durationSlider.value = actionData.duration;

        durationSlider.onValueChanged.AddListener(OnSliderValueChanged);

        viewButton.onClick.AddListener(PreviewAction);
        selectButton.onClick.AddListener(SelectAction);

        UpdateDurationText(durationSlider.value);
        selectButton.interactable = durationSlider.value > 0f;
    }

    void OnSliderValueChanged(float value)
    {
        UpdateDurationText(value);
        selectButton.interactable = value > 0f;
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
