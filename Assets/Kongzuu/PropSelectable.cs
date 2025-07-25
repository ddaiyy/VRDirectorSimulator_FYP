using UnityEngine;
using MyGame.Selection;

public class PropSelectable : MonoBehaviour, ICustomSelectable
{
    public GameObject propUI;

    public void OnSelect()
    {
        if (propUI != null)
        {
            propUI.SetActive(true);
            Debug.Log("道具被选中，显示 UI");
        }
    }

    public void OnDeselect()
    {
        if (propUI != null)
        {
            propUI.SetActive(false);
            Debug.Log("取消选中，隐藏 UI");
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
