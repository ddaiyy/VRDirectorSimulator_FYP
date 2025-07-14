using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSelector : MonoBehaviour
{
    public string actionName; // ±»»Á "Jump"°¢"Wave"

    public void OnSelected()
    {
        Debug.Log("Selected action: " + actionName);
        GameManager.Instance.SelectAction(actionName);
    }

    [ContextMenu("Test Select Action")]
    private void TestSelect()
    {
        OnSelected();
    }
}

