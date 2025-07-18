using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ToggleCanvasWithButton : MonoBehaviour
{
    public GameObject canvasToToggle; // 要显示/隐藏的 Canvas
    private bool isCanvasVisible = false;

    private InputDevice rightController;

    void Start()
    {
        if (canvasToToggle == null)
        {
            Debug.LogWarning("Canvas 未指定！");
        }

        // 获取右手控制器
        var rightHandedControllers = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandedControllers);

        if (rightHandedControllers.Count > 0)
        {
            rightController = rightHandedControllers[0];
        }
        else
        {
            Debug.LogWarning("未检测到右手控制器！");
        }

        // 初始化隐藏
        if (canvasToToggle != null)
            canvasToToggle.SetActive(false);
    }

    void Update()
    {
        if (rightController.isValid)
        {
            bool bButtonPressed = false;

            // 检测 B 按键是否被按下
            if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bButtonPressed) && bButtonPressed)
            {
                ToggleCanvas();
            }
        }
    }

    private float lastToggleTime = 0f;
    private float debounceTime = 0.5f; // 防抖间隔

    private void ToggleCanvas()
    {
        if (Time.time - lastToggleTime < debounceTime)
            return;

        isCanvasVisible = !isCanvasVisible;
        if (canvasToToggle != null)
        {
            canvasToToggle.SetActive(isCanvasVisible);
            Debug.Log("Canvas " + (isCanvasVisible ? "已显示" : "已隐藏"));
        }

        lastToggleTime = Time.time;
    }

    // 用于编辑器右键测试 Canvas 显示/隐藏
    [ContextMenu("测试切换 Canvas 显示状态")]
    public void TestToggleCanvas()
    {
        ToggleCanvas();
    }
}