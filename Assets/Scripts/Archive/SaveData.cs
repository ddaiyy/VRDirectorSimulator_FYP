using System;

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
