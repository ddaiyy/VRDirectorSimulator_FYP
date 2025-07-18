// CameraUI.cs
using UnityEngine;
using UnityEngine.UI;

public class CameraUI : MonoBehaviour
{
    public CameraController controller;

    // ����������������UIԤ����İ�ť OnClick �¼���
    public void OnSelectClicked()
    {
        if (controller != null)
        {
            Debug.Log($"Select clicked: {controller.gameObject.name}");
            controller.SelectThisCamera();
        }
    }

    public void OnDeleteClicked()
    {
        if (controller != null)
        {
            Debug.Log($"Delete clicked: {controller.gameObject.name}");
            controller.DeleteThisCamera();
            Destroy(gameObject); // ɾ����ǰUI���
        }
    }
}
