/*using UnityEngine;

public class CursorClickTogglePlane : MonoBehaviour
{
    public GameObject selectionPlanePrefab; // 拖入预制体
    private GameObject spawnedPlaneInstance;

    private bool isPlaneSpawned = false;

    [ContextMenu("Test SpawnPlane")]
    public void OnCursorClicked()
    {
        if (selectionPlanePrefab == null)
        {
            Debug.LogWarning("No prefab assigned!");
            return;
        }

        isPlaneSpawned = !isPlaneSpawned;

        if (isPlaneSpawned)
        {
            // 使用预制体原始位置和旋转（即 prefab 设计时的位置）
            Vector3 prefabPosition = selectionPlanePrefab.transform.position;
            Quaternion prefabRotation = selectionPlanePrefab.transform.rotation;

            spawnedPlaneInstance = Instantiate(selectionPlanePrefab, prefabPosition, prefabRotation);
            Debug.Log("Generated at the original position of the prefab.");
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
*/
using UnityEngine;

public class CursorClickTogglePlane : MonoBehaviour
{
    public GameObject selectionPlanePrefab; // 拖入预制体
    private GameObject spawnedPlaneInstance;
    private bool isPlaneSpawned = false;

    // 将此方法绑定到 UI 按钮的 OnClick 事件
    public void TogglePlaneByButton()
    {
        if (selectionPlanePrefab == null)
        {
            Debug.LogWarning("No prefab assigned!");
            return;
        }

        isPlaneSpawned = !isPlaneSpawned;

        if (isPlaneSpawned)
        {
            Vector3 prefabPosition = selectionPlanePrefab.transform.position;
            Quaternion prefabRotation = selectionPlanePrefab.transform.rotation;

            spawnedPlaneInstance = Instantiate(selectionPlanePrefab, prefabPosition, prefabRotation);
            Debug.Log("Generated at the original position of the prefab.");
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
