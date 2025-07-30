using UnityEngine;

public class LoadPlayerPosition : MonoBehaviour
{
    public Transform playerTransform;

    void Start()
    {
        string id = PlayerPrefs.GetString("CurrentSaveId", "");
        if (string.IsNullOrEmpty(id)) return;

        SaveData data = SaveSystem.Load(id);
        if (data == null) return;

        playerTransform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
    }
}
