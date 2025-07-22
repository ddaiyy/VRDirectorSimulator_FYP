using UnityEngine;
using UnityEngine.Android;

public class PermissionRequest : MonoBehaviour
{
    void Start()
    {
        // Android 10 ����ֻ��Ҫ MANAGE_EXTERNAL_STORAGE ��ʹ�� Scoped Storage��
        // ��Ϊ�˼����ԣ����� READ/WRITE Ȩ������
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
