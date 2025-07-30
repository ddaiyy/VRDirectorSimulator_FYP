using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class ToggleCanvasWithButton : MonoBehaviour
{
    public GameObject canvasToToggle;
    public Transform userHead; // 拖入 XR Origin 的 Main Camera
    public Vector3 offset = new Vector3(0.5f, -0.3f, 0.6f); // Canvas 相对头部右前偏移


    private InputDevice rightController;
    private bool isCanvasVisible = false;

    private float lastToggleTime = 0f;
    private float debounceTime = 0.5f;

    void OnEnable()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        TryInitializeRightController();
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            TryInitializeRightController();
        }

        if (rightController.isValid)
        {
            bool bButtonPressed = false;
            if (rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bButtonPressed) && bButtonPressed)
            {
                if (Time.time - lastToggleTime > debounceTime)
                {
                    ToggleCanvas();
                    lastToggleTime = Time.time;
                }
            }
        }
        // 🧭 跟随用户右侧
        if (isCanvasVisible && canvasToToggle != null && userHead != null)
        {
            Vector3 targetPos = userHead.position + userHead.right * offset.x
                                                 + userHead.up * offset.y
                                                 + userHead.forward * offset.z;

            canvasToToggle.transform.position = targetPos;
            canvasToToggle.transform.LookAt(userHead);
            canvasToToggle.transform.Rotate(0, 180, 0); // 反转使正面朝向用户
        }
    }

    void TryInitializeRightController()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        foreach (var device in devices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right) &&
                device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
            {
                rightController = device;
                Debug.Log("✅ 检测到右手控制器：" + device.name);
                break;
            }
        }
    }

    void OnDeviceConnected(InputDevice device)
    {
        if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right) &&
            device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
        {
            rightController = device;
            Debug.Log("📡 右手控制器连接：" + device.name);
        }
    }

    void ToggleCanvas()
    {
        isCanvasVisible = !isCanvasVisible;
        if (canvasToToggle != null)
            canvasToToggle.SetActive(isCanvasVisible);

        Debug.Log("Canvas " + (isCanvasVisible ? "已显示" : "已隐藏"));
    }


    // ✅ 右键菜单测试切换 Canvas
    [ContextMenu("测试切换 Canvas 显示状态")]
    public void TestToggleCanvas()
    {
        ToggleCanvas();
    }
}
