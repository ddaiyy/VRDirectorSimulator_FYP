/*using UnityEngine;

public class CursorClickTogglePlane : MonoBehaviour
{
    public GameObject selectionPlanePrefab; // 拖入预制体
    private static GameObject spawnedPlaneInstance;
    *//*private bool isPlaneSpawned = false;*//*
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
*/


using UnityEngine;
using System.Collections.Generic;

public class CursorClickTogglePlane : MonoBehaviour
{
    public GameObject selectionPlanePrefab; // 预制体
    public string spawnPointName = "SelectionPlaneSpawnPoint";
    public string planeKey; // 按钮类型，比如 "food", "modern", "ancient"

    // 静态字典：保存不同类型的平面实例
    private static Dictionary<string, GameObject> planeInstances = new Dictionary<string, GameObject>();

    public void TogglePlaneByButton()
    {
        Debug.Log($"Button {planeKey} clicked.");

        GameObject spawnPoint = GameObject.Find(spawnPointName);
        if (spawnPoint == null)
        {
            Debug.LogWarning("No spawn point found in scene!");
            return;
        }

        // 当前按钮对应的平面已经存在 → 销毁它
        if (planeInstances.ContainsKey(planeKey) && planeInstances[planeKey] != null)
        {
            Destroy(planeInstances[planeKey]);
            planeInstances[planeKey] = null;
            Debug.Log($"{planeKey} plane destroyed.");
        }
        else
        {
            // 先销毁所有其他平面
            foreach (var key in new List<string>(planeInstances.Keys))
            {
                if (planeInstances[key] != null)
                {
                    Destroy(planeInstances[key]);
                    planeInstances[key] = null;
                }
            }

            // 创建新平面
            GameObject newPlane = Instantiate(
                selectionPlanePrefab,
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );
            planeInstances[planeKey] = newPlane;
            Debug.Log($"{planeKey} plane generated.");
        }
    }
}
