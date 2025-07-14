using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSelector : MonoBehaviour
{
    public string environmentName;      // 环境名称
    public GameObject previewObject;    // 预览面板或模型

    public void OnHoverEnter()
    {
        if (previewObject != null)
            previewObject.SetActive(true);
        Debug.Log("Hover on environment: " + environmentName);
    }

    public void OnHoverExit()
    {
        if (previewObject != null)
            previewObject.SetActive(false);
    }

    public void OnSelected()
    {
        Debug.Log("Environment selected: " + environmentName);
        GameManager.Instance.SelectEnvironment(environmentName);
    }

    [ContextMenu("Test Select Action")]
    private void TestSelect()
    {
        OnSelected();
    }
}

