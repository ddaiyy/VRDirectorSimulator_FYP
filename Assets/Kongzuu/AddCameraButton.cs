using UnityEngine;

public class AddCameraButton : MonoBehaviour
{
    /*public GameObject selectionPlanePrefab; // 拖入预制体
    private GameObject spawnedPlaneInstance;*/

    [ContextMenu("Test SpawnPlane")]
    public void OnCursorClicked()
    {
        // 使用预制体原始位置和旋转（即 prefab 设计时的位置）
        /*Vector3 prefabPosition = selectionPlanePrefab.transform.position;
        Quaternion prefabRotation = selectionPlanePrefab.transform.rotation;

        spawnedPlaneInstance = Instantiate(selectionPlanePrefab, prefabPosition, prefabRotation);*/
        CameraManager.Instance.AddNewCamera();
        Debug.Log("Generated at the original position of the prefab.");
        
    }
    /*public void OnClick()
    {
        CameraManager.Instance.AddNewCamera();
    }*/
}
