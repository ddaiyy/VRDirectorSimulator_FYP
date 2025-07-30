/*using System;

[System.Serializable]
public class SaveData
{
    public string saveId;
    public string saveName;
    public string lastScene;
    public DateTime saveTime;

    // 可以扩展：比如位置、进度等
    public float playerX;
    public float playerY;
    public float playerZ;
}
*/

using System;
[System.Serializable]
public class SaveData
{
    public string saveId;
    public string saveName;
    public string lastScene;

    // 为了兼容 Unity 的 JsonUtility
    public string saveTimeString;

    public float playerX;
    public float playerY;
    public float playerZ;

    // 非序列化字段，仅用于运行时访问
    [System.NonSerialized]
    public System.DateTime saveTime;

    public void SyncSaveTimeToString()
    {
        saveTimeString = saveTime.ToString("o"); // ISO 8601 格式
    }

    public void SyncSaveTimeFromString()
    {
        if (!string.IsNullOrEmpty(saveTimeString))
            saveTime = System.DateTime.Parse(saveTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        else
            saveTime = System.DateTime.MinValue;
    }
}
