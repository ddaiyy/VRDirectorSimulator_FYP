using UnityEngine;
using UnityEngine.XR;

namespace MyGame.Selection
{
    public class ModelManipulator : MonoBehaviour, ICustomSelectable
    {
        public XRNode inputSource = XRNode.LeftHand;
        public float rotationSpeed = 60f;
        public float scaleSpeed = 0.5f;
        public float minScale = 0.2f;
        public float maxScale = 3f;

        private InputDevice device;
        private bool isSelected = false;

        void Start()
        {
            device = InputDevices.GetDeviceAtXRNode(inputSource);
        }

        void Update()
        {
            if (!isSelected) return;

            if (!device.isValid)
                device = InputDevices.GetDeviceAtXRNode(inputSource);

            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            {
                // 左右旋转
                if (Mathf.Abs(axis.x) > 0.1f)
                {
                    float angle = axis.x * rotationSpeed * Time.deltaTime;
                    transform.Rotate(Vector3.up, -angle, Space.World);
                }

                // 上下缩放
                if (Mathf.Abs(axis.y) > 0.1f)
                {
                    float scaleDelta = axis.y * scaleSpeed * Time.deltaTime;
                    Vector3 newScale = transform.localScale + Vector3.one * scaleDelta;
                    float clamped = Mathf.Clamp(newScale.x, minScale, maxScale);
                    transform.localScale = new Vector3(clamped, clamped, clamped);
                }
            }
        }

        public void OnSelect()
        {
            isSelected = true;
        }

        public void OnDeselect()
        {
            isSelected = false;
        }
    }
}
