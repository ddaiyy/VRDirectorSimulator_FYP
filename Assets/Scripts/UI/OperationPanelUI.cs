using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OperationPanelUI : MonoBehaviour
{
    public Button Switch2CameraPanelButton;
    
    void Start()
    {
        // 添加监听
        Switch2CameraPanelButton.onClick.AddListener(OnSwitch2CameraPanelButtonClicked);
    }

    // 点击后的反馈方法
    void OnSwitch2CameraPanelButtonClicked()
    {
        Debug.Log("切换到摄像机面板按钮被点击！");
        // 你可以在这里添加更多反馈，比如UI动画、声音、切换面板等
    }
}
