using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using static ModelUtils;

public class GrabScaleController_XRInput : MonoBehaviour
{
    [Header("控制器组件")]
    public XRBaseInteractor rightHandGrab;

    [Header("缩放参数")]
    public float scaleStep = 0.3f;
    public float minScale = 0.5f;
    public float maxScale = 8f;
    public float debounceTime = 0.3f;

    private InputDevice leftController;
    private Transform grabbedObject;
    // 添加字段缓存 Visual 子物体
    private Transform visualTarget;

    private float lastScaleTime = 0f;

    private void OnEnable()
    {
        rightHandGrab.selectEntered.AddListener(OnSelectEntered);
        rightHandGrab.selectExited.AddListener(OnSelectExited);
        InputDevices.deviceConnected += OnDeviceConnected;
        TryInitializeLeftController();
    }

    private void OnDisable()
    {
        rightHandGrab.selectEntered.RemoveListener(OnSelectEntered);
        rightHandGrab.selectExited.RemoveListener(OnSelectExited);
        InputDevices.deviceConnected -= OnDeviceConnected;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        grabbedObject = args.interactableObject.transform;
        visualTarget = GetVisualTarget(grabbedObject);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        grabbedObject = null;
        visualTarget = null;
    }


    void Update()
    {
        if (rightHandGrab == null)
        {
            Debug.LogWarning("⚠️ rightHandGrab 没有赋值！");
            return;
        }

        if (rightHandGrab.hasSelection)
        {
            if (rightHandGrab.firstInteractableSelected != null)
            {
                grabbedObject = rightHandGrab.firstInteractableSelected.transform;
            }

            if (grabbedObject == null) return; // 防止空对象

            if (visualTarget == null || visualTarget.parent != grabbedObject)
            {
                visualTarget = GetVisualTarget(grabbedObject);
            }

            if (visualTarget == null) return; // 找不到可缩放目标就直接退出

            if (Time.time - lastScaleTime > debounceTime)
            {
                bool xPressed = false;
                bool yPressed = false;

                leftController.TryGetFeatureValue(CommonUsages.primaryButton, out xPressed);
                leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out yPressed);

                if (xPressed)
                {
                    ScaleObject(-scaleStep);
                    lastScaleTime = Time.time;
                }
                else if (yPressed)
                {
                    ScaleObject(scaleStep);
                    lastScaleTime = Time.time;
                }
            }
        }
        else
        {
            grabbedObject = null;
            visualTarget = null;
        }
    }


    void ScaleObject(float delta)
    {
        if (visualTarget == null)
        {
            Debug.LogWarning("⚠️ 没有可缩放的目标！");
            return;
        }

        // 1. 缩放 Visual（模型显示）
        Vector3 oldScale = visualTarget.localScale;
        Vector3 newScale = oldScale + Vector3.one * delta;

        float x = Mathf.Clamp(newScale.x, minScale, maxScale);
        float y = Mathf.Clamp(newScale.y, minScale, maxScale);
        float z = Mathf.Clamp(newScale.z, minScale, maxScale);

        visualTarget.localScale = new Vector3(x, y, z);

        Debug.Log($"✅ 缩放对象: {visualTarget.name}, 原始缩放: {oldScale}, 当前缩放: {visualTarget.localScale}");

        // 2. 更新父物体（grabbedObject）的碰撞器和 XR 组件
        
        if (grabbedObject != null)
        {
            AddAccurateBoxCollider(grabbedObject.gameObject);

            // 自动注册 Collider 到 XRGrabInteractable
            var grab = grabbedObject.GetComponent<XRGrabInteractable>();
            var col = grabbedObject.GetComponent<BoxCollider>();
            if (grab != null && col != null)
            {
                if (!grab.colliders.Contains(col))
                {
                    grab.colliders.Clear();
                    grab.colliders.Add(col);
                    Debug.Log("✅ Collider 自动添加到 grab 列表中");
                }
            }
        }

    }




    // 尝试找到真正应该被缩放的模型（带有 MeshRenderer 的）
    Transform GetVisualTarget(Transform root)
    {
        // 如果它有叫 VisualRoot 的子物体，说明模型是标准结构，返回 root 本体
        var visualRoot = root.Find("VisualRoot");
        if (visualRoot != null)
            return root;

        // 否则尝试找到最上层有多个 renderer 的共同父节点
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 1)
        {
            // 找出最上层公共父物体
            Transform commonRoot = renderers[0].transform;
            foreach (var r in renderers)
            {
                commonRoot = FindCommonRoot(commonRoot, r.transform);
            }
            return commonRoot;
        }

        // fallback：单一 MeshRenderer 或 SkinnedMeshRenderer
        var mesh = root.GetComponentInChildren<MeshRenderer>();
        if (mesh != null) return mesh.transform;

        var skinned = root.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinned != null) return skinned.transform;

        return null;
    }
    Transform FindCommonRoot(Transform a, Transform b)
    {
        HashSet<Transform> ancestors = new HashSet<Transform>();
        while (a != null)
        {
            ancestors.Add(a);
            a = a.parent;
        }

        while (b != null)
        {
            if (ancestors.Contains(b))
                return b;
            b = b.parent;
        }

        return null;
    }





    void TryInitializeLeftController()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        foreach (var device in devices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left) &&
                device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
            {
                leftController = device;
                Debug.Log("✅ 检测到左手控制器：" + device.name);
                break;
            }
        }
    }

    void OnDeviceConnected(InputDevice device)
    {
        if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left) &&
            device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
        {
            leftController = device;
            Debug.Log("📡 左手控制器连接：" + device.name);
        }
    }

    [ContextMenu("测试放大")]
    public void TestScaleUp()
    {
        if (rightHandGrab.hasSelection)
            ScaleObject(scaleStep);
    }

    [ContextMenu("测试缩小")]
    public void TestScaleDown()
    {
        if (rightHandGrab.hasSelection)
            ScaleObject(-scaleStep);
    }
}