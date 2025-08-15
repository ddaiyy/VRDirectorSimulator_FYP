using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSlotUIManager : MonoBehaviour
{
    public GameObject saveSlotPrefab;
    public Transform slotContainer;

    void Start()
    {
        Debug.Log("正在加载所有存档按钮");
        LoadAllSlots();
    }

    public void LoadAllSlots()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        List<SaveData> all = SaveSystem.LoadAll();

        foreach (var save in all)
        {
            GameObject go = Instantiate(saveSlotPrefab, slotContainer);
            var slot = go.GetComponent<SaveSlotButton>();     
            slot.SetupExistingSlot(save);

        }

        if (all.Count < 5)
        {
            GameObject add = Instantiate(saveSlotPrefab, slotContainer);
            var addSlot = add.GetComponent<SaveSlotButton>();
            addSlot.SetupNewSlot();
        }
        else
        {
            GameObject add = Instantiate(saveSlotPrefab, slotContainer);
            var addSlot = add.GetComponent<SaveSlotButton>();
            addSlot.SetupNewSlot(disabled: true); // 👈 新增参数，表示按钮不能用
        }

    }

    [ContextMenu("清空所有存档")]
    public void ClearAllSavesForTest()
    {
        SaveSystem.DeleteAllSaves();
        LoadAllSlots();
    }

}
