using UnityEngine;
using TMPro;

public class OpenKeyboardTrigger : MonoBehaviour
{
    public VRKeyboardManager keyboardManager;
    private TMP_InputField inputField;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    public void OnClickInputField()
    {
        if (inputField == null)
        {
            Debug.LogError("❌ TMP_InputField 没挂！");
            return;
        }

        keyboardManager.ShowKeyboard(inputField);
    }
}
