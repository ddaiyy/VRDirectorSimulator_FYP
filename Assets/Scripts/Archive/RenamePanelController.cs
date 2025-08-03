using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RenamePanelController : MonoBehaviour
{
    public static RenamePanelController instance;

    public CanvasGroup panelGroup;
    public TMP_InputField inputField;

    private string newSaveId;

    private void Awake()
    {
        instance = this;
        HidePanel();
    }

    public void ShowPanel()
    {
        inputField.text = "";
        newSaveId = System.Guid.NewGuid().ToString();
        panelGroup.alpha = 1;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
    }

    public void HidePanel()
    {
        panelGroup.alpha = 0;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
    }

    public void ConfirmNewSave()
    {
        string inputName = inputField.text.Trim();
        if (string.IsNullOrEmpty(inputName))
        {
            Debug.LogWarning("项目名不能为空！");
            return;
        }

        if (inputName.Length > 10)
        {
            Debug.LogWarning("项目名不能超过10个字符！");
            inputName = inputName.Substring(0, 10); // 👈 截断为前10个字符
        }

        SaveData newData = new SaveData
        {
            saveId = newSaveId,
            saveName = inputName,
            lastScene = "Environment",
            saveTime = System.DateTime.Now,

            //保存当前用户在 Setting 里选的天空盒
            timeOfDayIndex = PlayerPrefs.GetInt("UserTimeOfDay", 0),
        };

        SaveSystem.Save(newData);
        PlayerPrefs.SetString("CurrentSaveId", newData.saveId);
        SceneManager.LoadScene("Environment");
    }

    public void CancelNewSave()
    {
        HidePanel();
    }
}