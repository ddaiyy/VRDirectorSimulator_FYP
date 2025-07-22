using UnityEngine;
using UnityEngine.Android;

public class PermissionRequest : MonoBehaviour
{
    void Start()
    {
        // Android 10 以上只需要 MANAGE_EXTERNAL_STORAGE 或使用 Scoped Storage，
        // 但为了兼容性，保留 READ/WRITE 权限请求
        RequestPermissions();
    }

    void RequestPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
    }
}
