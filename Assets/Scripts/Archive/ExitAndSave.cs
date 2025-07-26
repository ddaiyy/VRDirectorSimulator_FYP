using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitAndSave : MonoBehaviour
{
    public Transform playerTransform;

    public void SaveAndExit()
    {
        string id = PlayerPrefs.GetString("CurrentSaveId", "");
        if (string.IsNullOrEmpty(id)) return;

        SaveData data = SaveSystem.Load(id);
        if (data == null) return;

        data.lastScene = SceneManager.GetActiveScene().name;
        data.saveTime = System.DateTime.Now;
        data.playerX = playerTransform.position.x;
        data.playerY = playerTransform.position.y;
        data.playerZ = playerTransform.position.z;

        SaveSystem.Save(data);

        SceneManager.LoadScene("Start");
    }
}
