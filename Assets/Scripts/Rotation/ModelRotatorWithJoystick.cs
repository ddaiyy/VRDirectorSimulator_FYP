using UnityEngine;
using UnityEngine.XR;

namespace MyGame.Selection
{
    public class ModelRotatorWithJoystick : MonoBehaviour, ICustomSelectable
    {
        public XRNode inputSource = XRNode.LeftHand;
        public float rotationSpeed = 60f;

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
                if (Mathf.Abs(axis.x) > 0.1f)
                {
                    float angle = axis.x * rotationSpeed * Time.deltaTime;
                    transform.Rotate(Vector3.up, -angle, Space.World);
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
