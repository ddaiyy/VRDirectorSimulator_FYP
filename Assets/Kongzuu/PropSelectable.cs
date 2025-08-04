using UnityEngine;
using MyGame.Selection;

public class PropSelectable : MonoBehaviour, ICustomSelectable
{
    [Header("UI Canvas 预制体（需放入 Resources 或 Inspector 引用）")]
    public GameObject propUIPrefab;

    private GameObject spawnedUI;


    public void OnSelect()
    {
        Debug.Log("OnSelect() 被调用啦！");

        if (propUIPrefab != null && spawnedUI == null)
        {
            spawnedUI = Instantiate(propUIPrefab);

            spawnedUI.transform.position = transform.position + new Vector3(0, 1f, 0);
            spawnedUI.transform.rotation = Quaternion.identity;

            spawnedUI.SetActive(true);

            Debug.Log("UI 已实例化");
        }
        else if (spawnedUI != null)
        {
            Debug.Log("UI 已存在，重复点击不会重新生成");
        }
        else
        {
            Debug.LogWarning("propUIPrefab 没有绑定，无法实例化 UI");
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
