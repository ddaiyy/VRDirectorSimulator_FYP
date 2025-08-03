using UnityEngine;

public class LoadPlayerPosition : MonoBehaviour
{
    public Transform playerTransform;
    public SceneObjectManager sceneObjectManager;

    void Start()
    {
        string id = PlayerPrefs.GetString("CurrentSaveId", "");
        if (string.IsNullOrEmpty(id)) return;

        SaveData data = SaveSystem.Load(id);
        if (data == null) return;

        playerTransform.position = new Vector3(data.playerX, data.playerY, data.playerZ);

        DayNightManager.TimeOfDay time = (DayNightManager.TimeOfDay)data.timeOfDayIndex;
        FindObjectOfType<DayNightManager>()?.ApplyTimeOfDay(time);
        // ⚠️ 把存档里的值写回 PlayerPrefs，防止被 Setting 页面覆盖
        PlayerPrefs.SetInt("UserTimeOfDay", data.timeOfDayIndex);
        PlayerPrefs.Save();

        SceneObjectManager.Instance?.LoadObjects(data); // 👈 加载动态物体
    }
}
