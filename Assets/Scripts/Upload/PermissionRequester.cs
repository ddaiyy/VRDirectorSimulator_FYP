using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using System.Collections;

public class PermissionRequester : MonoBehaviour
{
    public System.Action OnPermissionGranted;

    void Start()
    {
        StartCoroutine(RequestPermissionCoroutine());
    }

    IEnumerator RequestPermissionCoroutine()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);

            // 等待授权结果
            while (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                yield return null;
            }
        }
#endif
        Debug.Log("读取存储权限已授权");
        OnPermissionGranted?.Invoke();
    }
}