using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MyGame.Selection
{
    public class ModelRotatorWithJoystick : MonoBehaviour, ICustomSelectable
    {
        public XRNode inputSource = XRNode.LeftHand;    // 左手控制旋转
        public XRNode buttonHand = XRNode.LeftHand;     // 左手按钮控制缩放
        public float rotationSpeed = 60f;
        public float scaleSpeed = 0.5f;

        private InputDevice device;
        private InputDevice buttonDevice;

        private bool isSelected = false;

        [Header("UI Prefab，在 Inspector 里拖拽")]
        [SerializeField] public GameObject currentCanvasInstance;

        void Start()
        {
            device = InputDevices.GetDeviceAtXRNode(inputSource);
            buttonDevice = InputDevices.GetDeviceAtXRNode(buttonHand);

            TimelineTrack timelineTrack = gameObject.GetComponent<TimelineTrack>();
            if (timelineTrack != null)
            {
                if (timelineTrack.objectTimelineUI != null)
                {
                    currentCanvasInstance = timelineTrack.objectTimelineUI.gameObject;
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name} 的 TimelineTrack.objectTimelineUI 是 null");
                }
            }
        }


        void Update()
        {
            if (!isSelected) return;

            if (!device.isValid)
                device = InputDevices.GetDeviceAtXRNode(inputSource);

            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(inputSource, devices);

            if (!buttonDevice.isValid)
                buttonDevice = InputDevices.GetDeviceAtXRNode(buttonHand);

            // 左手摇杆旋转物体
            if (devices.Count > 0)
            {
                device = devices[0];
            }

            // 旋转逻辑
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            {
                if (Mathf.Abs(axis.x) > 0.1f)
                {
                    float angle = axis.x * rotationSpeed * Time.deltaTime;
                    transform.Rotate(Vector3.up, -angle, Space.World);
                }
            }

            // 左手 Y 按钮（放大）
            
            if (buttonDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryPressed) && secondaryPressed)
            {
                ScaleObject(1 - scaleSpeed * Time.deltaTime);
            }

            // 左手 X 按钮（缩小）
            if (buttonDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed) && primaryPressed)
            {
                ScaleObject(1 + scaleSpeed * Time.deltaTime);
            }

        }

        public void OnSelect()
        {
            isSelected = true;

            // 如果 currentCanvasInstance 还没赋值，就尝试重新获取
            if (currentCanvasInstance == null)
            {
                var timelineTrack = GetComponent<TimelineTrack>();
                if (timelineTrack != null)
                {
                    // 等待 Canvas 被生成（可能是 TimelineTrack 自己生成的）
                    if (timelineTrack.objectTimelineUI != null)
                    {
                        currentCanvasInstance = timelineTrack.objectTimelineUI.gameObject;
                    }
                    else
                    {
                        Debug.LogWarning($"{gameObject.name} 的 TimelineTrack 还没生成 UI，无法赋值");
                    }
                }
            }


            // 如果还是没赋值就报错
            if (currentCanvasInstance == null)
            {
                Debug.LogError($"❌ {gameObject.name} 没有找到 Canvas！");
                return;
            }

            // 只负责显示，不会再次点击时关闭
            if (!currentCanvasInstance.activeSelf)
            {
                currentCanvasInstance.SetActive(true);

                Vector3 offset = new Vector3(0, 1.5f, 0);
                currentCanvasInstance.transform.position = transform.position + offset;

                if (Camera.main != null)
                {
                    currentCanvasInstance.transform.LookAt(Camera.main.transform);
                    currentCanvasInstance.transform.Rotate(0, 180f, 0);
                }

                var uiController = currentCanvasInstance.GetComponent<PropUIController>();
                if (uiController != null)
                {
                    uiController.SetTarget(this.gameObject);
                }

                Debug.Log($"{gameObject.name} 被选中，显示 UI");
            }
            else
            {
                Debug.Log($"{gameObject.name} 已选中且 UI 已显示，再次点击不会关闭 UI");
            }
        }


        public void OnDeselect()
        {
            isSelected = false;
            //canvas开启后不会再隐藏

            /*if (currentCanvasInstance != null && currentCanvasInstance.activeSelf)
            {
                currentCanvasInstance.SetActive(false);
                Debug.Log($"{gameObject.name} 被取消选择，隐藏 UI");
            }*/
        }
        private void ScaleObject(float scaleFactor)
        {
            Vector3 newScale = transform.localScale * scaleFactor;
            newScale = ClampVector3(newScale, 0.1f, 5f); // 限制缩放范围
            transform.localScale = newScale;
        }

        private Vector3 ClampVector3(Vector3 v, float min, float max)
        {
            return new Vector3(
                Mathf.Clamp(v.x, min, max),
                Mathf.Clamp(v.y, min, max),
                Mathf.Clamp(v.z, min, max)
            );
        }
    }
}
