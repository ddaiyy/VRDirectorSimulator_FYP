using UnityEngine;
using UnityEngine.Android;

public class AndroidPermission : MonoBehaviour
{
    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
    }
}
