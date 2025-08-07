using UnityEngine;

public class PropUIController : MonoBehaviour
{
    private GameObject targetObject;

    public void SetTarget(GameObject target)
    {
        targetObject = target;
    }

    // Delete 按钮绑定这个方法
    public void OnDeleteClicked()
    {
        if (targetObject != null)
        {
            Debug.Log("🗑 删除物体: " + targetObject.name);
            Destroy(targetObject);   // 删除模型
        }
        Destroy(gameObject);         // 删除 UI
    }
}
