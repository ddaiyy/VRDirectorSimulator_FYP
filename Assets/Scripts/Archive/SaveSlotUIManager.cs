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

    void LoadAllSlots()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        List<SaveData> all = SaveSystem.LoadAll();

        foreach (var save in all)
        {
            GameObject go = Instantiate(saveSlotPrefab, slotContainer);
            var slot = go.GetComponent<SaveSlotButton>();
            /*slot.label.text = save.saveName;
            slot.saveId = save.saveId;
            slot.isNewSlot = false;*/
            slot.SetupExistingSlot(save);

        }

        // 最后添加一个 + 新建按钮
        GameObject add = Instantiate(saveSlotPrefab, slotContainer);
        var addSlot = add.GetComponent<SaveSlotButton>();
        //addSlot.label.text = "+";
        //addSlot.isNewSlot = true;
        addSlot.SetupNewSlot();
    }

    [ContextMenu("清空所有存档")]
    public void ClearAllSavesForTest()
    {
        SaveSystem.DeleteAllSaves();
        LoadAllSlots();
    }

}
