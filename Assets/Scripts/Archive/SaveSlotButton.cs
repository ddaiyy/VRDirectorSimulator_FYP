using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveSlotButton : MonoBehaviour
{
    public TMP_Text label;
    public string saveId;
    public bool isNewSlot;

    private SaveData currentData;
    public GameObject deleteButtonGO; // 拖入 DeleteButton


    public void SetupExistingSlot(SaveData data)
    {
        Debug.Log("SetupExistingSlot 被调用: " + data.saveName);
        currentData = data;
        saveId = data.saveId;
        isNewSlot = false;

        if (label != null)
            label.text = $"{data.saveName}: {data.lastScene}\n{data.saveTime:MM/dd HH:mm}";

        if (deleteButtonGO != null)
            deleteButtonGO.SetActive(true);
    }

    public void SetupNewSlot(bool disabled = false)
    {
        currentData = null;
        isNewSlot = true;
        saveId = "";

        if (label != null)
            label.text = disabled ? "Archive is full" : "+";

        GetComponent<UnityEngine.UI.Button>().interactable = !disabled;

        if (deleteButtonGO != null)
            deleteButtonGO.SetActive(false);
    }


    public void OnClick()
    {
        Debug.Log($"按钮被点击，isNewSlot: {isNewSlot}, saveId: {saveId}");

        if (isNewSlot)
        {
            string newSaveName = "P " + (SaveSystem.LoadAll().Count + 1);
            // 创建新存档并跳转
            SaveData newData = new SaveData
            {
                saveId = System.Guid.NewGuid().ToString(),
                /*saveName = "New Save " + System.DateTime.Now.ToString("HH:mm"),*/
                saveName = newSaveName,
                lastScene = "Environment",
                saveTime = System.DateTime.Now
            };

            SaveSystem.Save(newData);
            PlayerPrefs.SetString("CurrentSaveId", newData.saveId);
            SceneManager.LoadScene("Environment");
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

    public void OnDeleteButtonClick()
    {
        if (currentData == null) return;

        // 简单确认对话框（可换成你自己的 UI）
        bool confirmed = true; // 你可以改成弹窗确认
        if (confirmed)
        {
            SaveSystem.Delete(currentData.saveId);
            // 刷新整个列表
            FindObjectOfType<SaveSlotUIManager>().LoadAllSlots();
        }
    }



}
