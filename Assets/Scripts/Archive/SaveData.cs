/*using System;

[System.Serializable]
public class SaveData
{
    public string saveId;
    public string saveName;
    public string lastScene;
    public DateTime saveTime;

    // ������չ������λ�á����ȵ�
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

    // Ϊ�˼��� Unity �� JsonUtility
    public string saveTimeString;

    public float playerX;
    public float playerY;
    public float playerZ;

    // �����л��ֶΣ�����������ʱ����
    [System.NonSerialized]
    public System.DateTime saveTime;

    public void SyncSaveTimeToString()
    {
        saveTimeString = saveTime.ToString("o"); // ISO 8601 ��ʽ
    }

    public void SyncSaveTimeFromString()
    {
        if (!string.IsNullOrEmpty(saveTimeString))
            saveTime = System.DateTime.Parse(saveTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        else
            saveTime = System.DateTime.MinValue;
    }
}
