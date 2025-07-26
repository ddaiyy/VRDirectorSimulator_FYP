using System;

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
