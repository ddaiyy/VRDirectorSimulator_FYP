using UnityEngine;

public class AddCameraButton : MonoBehaviour
{
    /*public GameObject selectionPlanePrefab; // 拖入预制体
    private GameObject spawnedPlaneInstance;*/

    [ContextMenu("Test SpawnPlane")]
    public void OnCursorClicked()
    {
        GameObject newCam = CameraManager.Instance.AddNewCamera();
        if (newCam != null && Camera.main != null)
        {
            // 将摄像机移动到玩家眼前 2 米
            newCam.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 3f;
            newCam.transform.rotation = Camera.main.transform.rotation;
            Debug.Log("Camera spawned in front of player.");
        }

        FeedbackManager.Instance.ShowMessage("Add Camera Successfully", MessageType.Success);
    }


}
