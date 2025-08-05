using UnityEngine;

public class CursorClickTogglePlane : MonoBehaviour
{
    public GameObject selectionPlanePrefab; // 拖入预制体
    private GameObject spawnedPlaneInstance;
    private bool isPlaneSpawned = false;
    public string spawnPointName = "SelectionPlaneSpawnPoint"; // 各场景共用的锚点名

    // 将此方法绑定到 UI 按钮的 OnClick 事件
    public void TogglePlaneByButton()
    {
        if (selectionPlanePrefab == null)
        {
            Debug.LogWarning("No prefab assigned!");
            return;
        }

        isPlaneSpawned = !isPlaneSpawned;

        /*if (isPlaneSpawned)
        {
            Vector3 prefabPosition = selectionPlanePrefab.transform.position;
            Quaternion prefabRotation = selectionPlanePrefab.transform.rotation;

            spawnedPlaneInstance = Instantiate(selectionPlanePrefab, prefabPosition, prefabRotation);
            Debug.Log("Generated at the original position of the prefab.");
        }*/
        if (isPlaneSpawned)
        {
            // 在场景中查找锚点
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

            Debug.Log("Generated at spawn point in scene.");
        }
        else
        {
            if (spawnedPlaneInstance != null)
            {
                Destroy(spawnedPlaneInstance);
                Debug.Log("Destroyed.");
            }
        }
    }
}
