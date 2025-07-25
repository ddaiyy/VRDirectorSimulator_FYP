using UnityEngine;

public class AddCameraButton : MonoBehaviour
{
    public void OnClick()
    {
        CameraManager.Instance.AddNewCamera();
    }
}
