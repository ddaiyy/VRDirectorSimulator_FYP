using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            label.text = $"{data.saveName}\n{data.saveTime:MM/dd HH:mm}";


        if (deleteButtonGO != null)
            deleteButtonGO.SetActive(true);

        // 重新绑定按钮点击事件，调用 OnClick()
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }
    }
    public void SetupTutorialSlot()
    {
        currentData = null;
        isNewSlot = false;
        saveId = "";

        if (label != null)
            label.text = "Tutorial";

        // 禁用删除按钮
        if (deleteButtonGO != null)
            deleteButtonGO.SetActive(false);

        // 使按钮始终可点（默认是Button组件）
        var btn = GetComponent<UnityEngine.UI.Button>();
        if (btn != null)
        {
            btn.interactable = true;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                // 直接跳转到Tutorial场景
                SceneManager.LoadScene("Select"); // 改成你Tutorial场景名
            });
        }
    }


    public void SetupNewSlot(bool disabled = false)
    {
        currentData = null;
        isNewSlot = true;
        saveId = "";

        if (label != null)
            label.text = disabled ? "Archive is full" : "+";

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = !disabled;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }

        //GetComponent<UnityEngine.UI.Button>().interactable = !disabled;

        if (deleteButtonGO != null)
            deleteButtonGO.SetActive(false);
    }


    public void OnClick()
    {      
        if (isNewSlot)
        {
            // 弹出改名面板，而不是立刻跳转
            RenamePanelController.instance.ShowPanel();
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