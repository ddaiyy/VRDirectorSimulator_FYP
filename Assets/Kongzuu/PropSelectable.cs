using UnityEngine;
using MyGame.Selection;

public class PropSelectable : MonoBehaviour, ICustomSelectable
{
    public GameObject propUI;

    public void OnSelect()
    {
        propUI?.SetActive(true);
        Debug.Log("���߱�ѡ��");
    }

    public void OnDeselect()
    {
        propUI?.SetActive(false);
        Debug.Log("����ȡ��ѡ��");
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
