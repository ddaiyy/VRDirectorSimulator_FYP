using UnityEngine;
using MyGame.Selection;

public class PropSelectable : MonoBehaviour, ICustomSelectable
{
    [Header("UI Canvas 预制体（需放入 Resources 或 Inspector 引用）")]
    public GameObject propUIPrefab;

    private GameObject spawnedUI;

    public void OnSelect()
    {
        if (propUIPrefab != null && spawnedUI == null)
        {
            // 实例化Canvas，不设置父物体
            spawnedUI = Instantiate(propUIPrefab);

            // 设置位置，和物体位置匹配，带偏移
            spawnedUI.transform.position = transform.position + new Vector3(0, 1f, 0);

            // 设置旋转，如果你想让UI不旋转，可以用Quaternion.identity
            spawnedUI.transform.rotation = Quaternion.identity;

            spawnedUI.SetActive(true);

            Debug.Log("道具被选中，UI已实例化，且不作为子物体");
        }
    }



    public void OnDeselect()
    {
        if (spawnedUI != null)
        {
            Destroy(spawnedUI);
            spawnedUI = null;
            Debug.Log("取消选中，UI 已销毁");
        }
    }

    [ContextMenu("Test OnSelect")]
    private void TestOnSelect()
    {
        OnSelect();
    }

    [ContextMenu("Test OnDeselect")]
    private void TestOnDeselect()
    {
        OnDeselect();
    }
}
