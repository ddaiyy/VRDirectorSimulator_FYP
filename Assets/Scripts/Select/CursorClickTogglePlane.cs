/*using UnityEngine;
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
*/

using UnityEngine;
using System.Collections.Generic;

public class CursorClickTogglePlane : MonoBehaviour
{
    public GameObject selectionPlanePrefab; // 预制体
    public Transform playerCamera; // XR Origin 的 Main Camera
    public float forwardDistance = 8f; // 平面距离玩家前方的水平距离
    public string planeKey; // 按钮类型，比如 "food", "modern", "ancient"
    public LayerMask groundLayer; // 指定地面层（避免打到其他物体）
    public float heightOffset = 0.01f; // 平面相对地面的高度偏移

    private static Dictionary<string, GameObject> planeInstances = new Dictionary<string, GameObject>();

    public void TogglePlaneByButton()
    {
        Debug.Log($"Button {planeKey} clicked.");

        if (playerCamera == null)
        {
            Debug.LogWarning("No player camera assigned!");
            return;
        }

        // 当前按钮对应的平面已经存在 → 销毁它
        if (planeInstances.ContainsKey(planeKey) && planeInstances[planeKey] != null)
        {
            Destroy(planeInstances[planeKey]);
            planeInstances[planeKey] = null;
            Debug.Log($"{planeKey} plane destroyed.");
            return;
        }

        // 先销毁所有其他平面
        foreach (var key in new List<string>(planeInstances.Keys))
        {
            if (planeInstances[key] != null)
            {
                Destroy(planeInstances[key]);
                planeInstances[key] = null;
            }
        }

        // 计算玩家眼前的水平位置
        Vector3 forwardPos = playerCamera.position + playerCamera.forward * forwardDistance;

        // 射线检测：从高处往下射线，找到地面
        Vector3 rayOrigin = forwardPos + Vector3.up * 5f; // 从高处往下
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            forwardPos.y = hit.point.y + heightOffset; // 把 y 设置成地面高度
        }
        else
        {
            Debug.LogWarning("No ground detected in front of player. Using current Y.");
        }

        // 创建平面
        GameObject newPlane = Instantiate(
            selectionPlanePrefab,
            forwardPos,
            Quaternion.identity // 你可以改成面向玩家
        );

        planeInstances[planeKey] = newPlane;
        Debug.Log($"{planeKey} plane generated.");
    }
}
