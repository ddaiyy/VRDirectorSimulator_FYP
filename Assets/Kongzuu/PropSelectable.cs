using UnityEngine;
using MyGame.Selection;

public class PropSelectable : MonoBehaviour, ICustomSelectable
{
    public GameObject propUI;

    public void OnSelect()
    {
        propUI?.SetActive(true);
        Debug.Log("道具被选中");
    }

    public void OnDeselect()
    {
        propUI?.SetActive(false);
        Debug.Log("道具取消选中");
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
