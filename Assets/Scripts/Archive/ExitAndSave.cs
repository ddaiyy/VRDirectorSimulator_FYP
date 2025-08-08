using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ExitAndSave : MonoBehaviour
{
    public Transform playerTransform;
    public SceneObjectManager sceneObjectManager;

    public GameObject saveNoticeUI; // 👈 UI 面板（比如一个 Text）提示“已自动保存”

    public void SaveAndExit()
    {
        StartCoroutine(SaveAndExitCoroutine());
    }

    private IEnumerator SaveAndExitCoroutine()
    {
        string id = PlayerPrefs.GetString("CurrentSaveId", "");
        if (string.IsNullOrEmpty(id)) yield break;

        SaveData data = SaveSystem.Load(id);
        if (data == null) yield break;

        // 保存位置信息和当前场景名
        data.lastScene = SceneManager.GetActiveScene().name;
        data.saveTime = System.DateTime.Now;
        data.playerX = playerTransform.position.x;
        data.playerY = playerTransform.position.y;
        data.playerZ = playerTransform.position.z;
        data.timeOfDayIndex = PlayerPrefs.GetInt("UserTimeOfDay", 0);

        // 保存场景动态对象
        SceneObjectManager.Instance?.SaveObjects(data);

        // 写入保存文件
        SaveSystem.Save(data);

        // 👇 显示“已自动保存”UI
        if (saveNoticeUI != null)
        {
            saveNoticeUI.SetActive(true);
        }

        // 👇 等待两秒
        yield return new WaitForSeconds(1f);

        // 👇 加载 Start 场景
        SceneManager.LoadScene("Start");
    }
}
