using UnityEngine;

public class CursorClickTogglePlane : MonoBehaviour
{
    public GameObject selectionPlanePrefab; // 拖入预制体
    private static GameObject spawnedPlaneInstance;
    /*private bool isPlaneSpawned = false;*/
    public string spawnPointName = "SelectionPlaneSpawnPoint"; // 各场景共用的锚点名

    // 将此方法绑定到 UI 按钮的 OnClick 事件
    public void TogglePlaneByButton()
    {
        Debug.Log("TogglePlaneByButton clicked.");
        if (selectionPlanePrefab == null)
        {
            Debug.LogWarning("No prefab assigned!");
            return;
        }

        // 如果平面已经生成，则销毁它
        if (spawnedPlaneInstance != null)
        {
            Debug.Log("Destroying: " + spawnedPlaneInstance.name);
            Destroy(spawnedPlaneInstance);
            spawnedPlaneInstance = null;
            Debug.Log("Plane destroyed.");
        }
        else // 否则，创建一个新平面
        {
            GameObject spawnPoint = GameObject.Find(spawnPointName);
            if (spawnPoint == null)
            {
                Debug.LogWarning("No spawn point found in scene!");
                return;
            }

            spawnedPlaneInstance = Instantiate(
                selectionPlanePrefab,
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );

            Debug.Log("Plane generated at spawn point.");
        }
    }
}
