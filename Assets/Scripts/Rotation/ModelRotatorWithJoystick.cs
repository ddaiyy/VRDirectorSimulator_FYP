using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;

namespace MyGame.Selection
{
    public enum SelectableType { Item, Character }
    public class ModelRotatorWithJoystick : MonoBehaviour, ICustomSelectable
    {
        [Header("对象类型")]
        public SelectableType type;

        [Header("角色控制器（人物类型专用）")]
        public CharacterActionController controllerForCharacter;
        public Transform characterTransform;

        [Header("UI 设置")]
        public GameObject ActionUI; // 动作ui
        public GameObject currentCanvasInstance;
        public GameObject currentPropCanvasInstance;


        [Header("XR 控制设置")]
        public XRNode inputSource = XRNode.LeftHand;// 左手控制旋转
        public XRNode buttonHand = XRNode.LeftHand;// 左手按钮控制缩放
        public float rotationSpeed = 60f;
        public float scaleSpeed = 0.5f;
        private InputDevice device;
        private InputDevice buttonDevice;

        private bool isSelected = false;


        void Start()
        {
            device = InputDevices.GetDeviceAtXRNode(inputSource);
            buttonDevice = InputDevices.GetDeviceAtXRNode(buttonHand);
        }


        void Update()
        {
            if (!isSelected) return;

            // 确保设备有效
            if (!device.isValid)
                device = InputDevices.GetDeviceAtXRNode(inputSource);

            if (!buttonDevice.isValid)
                buttonDevice = InputDevices.GetDeviceAtXRNode(buttonHand);

            // 旋转（左手摇杆）
            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            {
                if (Mathf.Abs(axis.x) > 0.1f)
                {
                    float angle = axis.x * rotationSpeed * Time.deltaTime;
                    transform.Rotate(Vector3.up, -angle, Space.World);
                }
            }

            // 缩放（左手XY按钮）
            if (buttonDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryPressed) && secondaryPressed)
            {
                ScaleObject(1 + scaleSpeed * Time.deltaTime);
            }
            if (buttonDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryPressed) && primaryPressed)
            {
                ScaleObject(1 - scaleSpeed * Time.deltaTime);
            }

        }

        public void OnSelect()
        {
            isSelected = true;

            if (type == SelectableType.Item)
            {
                HandlePropCanvas();
            }
            // 如果是人物，执行角色选择
            if (type == SelectableType.Character)
            {
                if (controllerForCharacter != null)
                {
                    SelectedCharacterManager.CurrentSelectedCharacter = controllerForCharacter;
                    Debug.Log($"✅ 选中了角色: {controllerForCharacter.gameObject.name}");
                }
            }

            // 只要 currentCanvasInstance 为空或者不激活，都创建/显示它
            if (currentCanvasInstance == null || !currentCanvasInstance.activeSelf)
            {
                HandleCharacterCanvas();
            }
        }

        private void HandleCharacterCanvas()
        {
            if (ActionUI == null || characterTransform == null)
            {
                Debug.LogWarning("人物UI或者Transform未设置");
                return;
            }

            // 第一个 Canvas：ActionUI
            if (currentCanvasInstance == null || !currentCanvasInstance.activeSelf)
            {
                currentCanvasInstance = Instantiate(ActionUI);
                Vector3 offset = new Vector3(2f, 2f, 0);
                currentCanvasInstance.transform.position = characterTransform.position + offset;

                if (Camera.main != null)
                {
                    currentCanvasInstance.transform.LookAt(Camera.main.transform);
                    currentCanvasInstance.transform.Rotate(0, 180f, 0);
                }

                // 不再作为人物子物体
                currentCanvasInstance.transform.SetParent(null); // 或者 SetParent(UICanvasRootTransform, false)
            }

            // 第二个 Canvas：TrackUI
            TimelineTrack track = GetComponent<TimelineTrack>();
            if (track != null && track.objectTimelineUI != null)
            {
                GameObject trackUI = track.objectTimelineUI.gameObject;
                if (!trackUI.activeSelf)
                {
                    trackUI.SetActive(true);
                    Vector3 offset2 = new Vector3(0, 1.5f, 0);
                    trackUI.transform.position = characterTransform.position + offset2;

                    if (Camera.main != null)
                    {
                        trackUI.transform.LookAt(Camera.main.transform);
                        trackUI.transform.Rotate(0, 180f, 0);
                    }

                    trackUI.transform.SetParent(null); // 保证它独立
                }
            }
        }


        private void HandlePropCanvas()
        {
            TimelineTrack timelineTrack = GetComponent<TimelineTrack>();
            if (timelineTrack != null && timelineTrack.objectTimelineUI != null)
            {
                if (currentPropCanvasInstance == null)
                {
                    currentPropCanvasInstance = timelineTrack.objectTimelineUI.gameObject;
                }

                if (!currentPropCanvasInstance.activeSelf)
                {
                    currentPropCanvasInstance.SetActive(true);
                    Vector3 offset = new Vector3(0, 1.5f, 0);
                    currentPropCanvasInstance.transform.position = transform.position + offset;

                    if (Camera.main != null)
                    {
                        currentPropCanvasInstance.transform.LookAt(Camera.main.transform);
                        currentPropCanvasInstance.transform.Rotate(0, 180f, 0);
                    }

                    // 不作为物体子物体，直接放到场景根节点或指定父物体
                    currentPropCanvasInstance.transform.SetParent(null);
                    // 或者用指定的Canvas父物体，比如：
                    // currentCanvasInstance.transform.SetParent(someUICanvasRootTransform);
                }

            }
            else
            {
                Debug.LogWarning("道具TimelineTrack 或 UI 未设置");
            }
        }

        public void OnDeselect()
        {
            isSelected = false;

        }
        private void ScaleObject(float scaleFactor)
        {
            Vector3 newScale = transform.localScale * scaleFactor;
            newScale = ClampVector3(newScale, 0.1f, 5f);
            transform.localScale = newScale;
        }
        private Vector3 ClampVector3(Vector3 v, float min, float max)
        {
            return new Vector3(
                Mathf.Clamp(v.x, min, max),
                Mathf.Clamp(v.y, min, max),
                Mathf.Clamp(v.z, min, max));
        }
    }
}