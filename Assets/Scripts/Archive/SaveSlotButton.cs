using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveSlotButton : MonoBehaviour
{
    public TMP_Text label;
    public string saveId;
    public bool isNewSlot;

    private SaveData currentData;

    public void SetupExistingSlot(SaveData data)
    {
        Debug.Log("SetupExistingSlot 被调用: " + data.saveName);
        currentData = data;
        saveId = data.saveId;
        isNewSlot = false;

        if (label != null)
            /*label.text = data.saveName + "\n" + data.saveTime.ToString("g");*/
            label.text = "";  // 不显示任何内容

    }

    public void SetupNewSlot()
    {
        currentData = null;
        isNewSlot = true;
        saveId = "";

        if (label != null)
            label.text = "+";
    }

    public void OnClick()
    {
        Debug.Log($"按钮被点击，isNewSlot: {isNewSlot}, saveId: {saveId}");

        if (isNewSlot)
        {
            // 创建新存档并跳转
            SaveData newData = new SaveData
            {
                saveId = System.Guid.NewGuid().ToString(),
                saveName = "New Save " + System.DateTime.Now.ToString("HH:mm"),
                lastScene = "EnvironmentSelect",
                saveTime = System.DateTime.Now
            };

            SaveSystem.Save(newData);
            PlayerPrefs.SetString("CurrentSaveId", newData.saveId);
            SceneManager.LoadScene("EnvironmentSelect");
        }
        else
        {
            if (currentData == null)
            {
                Debug.LogError("当前存档数据为空（currentData is null），请检查是否调用了 SetupExistingSlot。");
                return;
            }

            PlayerPrefs.SetString("CurrentSaveId", currentData.saveId);
            SceneManager.LoadScene(currentData.lastScene);
        }
    }


}
