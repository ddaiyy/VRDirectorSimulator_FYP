// CameraUI.cs
using UnityEngine;
using UnityEngine.UI;

public class CameraUI : MonoBehaviour
{
    public CameraController controller;

    // 这两个方法，绑定在UI预制体的按钮 OnClick 事件里
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
            Destroy(gameObject); // 删除当前UI面板
        }
    }
}
