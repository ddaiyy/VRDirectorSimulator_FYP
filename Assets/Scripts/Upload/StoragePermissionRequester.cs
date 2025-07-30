using UnityEngine;
using UnityEngine.Android;

public class StoragePermissionRequester : MonoBehaviour
{
    void Start()
    {
        RequestStoragePermission();
    }

    void RequestStoragePermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            Permission.RequestUserPermission(Permission.ExternalStorageRead);

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
    }
}
