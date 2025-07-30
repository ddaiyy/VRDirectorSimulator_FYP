using UnityEngine;
using TMPro;
using VRKeys;

public class VRKeyboardManager : MonoBehaviour
{
    public Keyboard vrKeyboard;

    private TMP_InputField currentInputField;

    private void Start()
    {
        vrKeyboard.OnUpdate.AddListener(HandleTextUpdate);
        vrKeyboard.OnSubmit.AddListener(HandleSubmit);
        vrKeyboard.OnCancel.AddListener(HandleCancel);
    }

    public void ShowKeyboard(TMP_InputField targetInputField)
    {
        Debug.Log("👉 ShowKeyboard 被调用了");
        if (targetInputField == null)
        {
            Debug.LogError("❌ 传入的 TMP_InputField 是 null！");
            return;
        }

        currentInputField = targetInputField;

        Debug.Log("✅ 设置文本：" + currentInputField.text);
        vrKeyboard.SetText(currentInputField.text);
        vrKeyboard.SetPlaceholderMessage("请输入内容...");
        vrKeyboard.Enable(); // 👈 关键
        Debug.Log("✅ 键盘 Enable 被调用！");
    }

    private void HandleTextUpdate(string updatedText)
    {
        if (currentInputField != null)
        {
            currentInputField.text = updatedText;
        }
    }

    private void HandleSubmit(string submittedText)
    {
        if (currentInputField != null)
        {
            currentInputField.text = submittedText;
        }

        vrKeyboard.Disable();
    }

    private void HandleCancel()
    {
        vrKeyboard.Disable();
    }
}
