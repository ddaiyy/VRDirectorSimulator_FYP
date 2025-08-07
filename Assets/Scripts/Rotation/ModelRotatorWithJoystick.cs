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
        [Header("UI Prefab，在 Inspector 里拖拽")]
        //public GameObject propUIPrefab;
        [SerializeField] private GameObject currentCanvasInstance;
        void Start()
        {
            device = InputDevices.GetDeviceAtXRNode(inputSource);
            TimelineTrack timelineTrack= gameObject.GetComponent<TimelineTrack>();
            if (timelineTrack != null)
            {
                currentCanvasInstance = timelineTrack.objectTimelineUI.gameObject;
            }
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

                // 给 UI 控制器传递目标
                var uiController = currentCanvasInstance.GetComponent<PropUIController>();
                if (uiController != null)
                {
                    uiController.SetTarget(this.gameObject);
                }

                Debug.Log($"{gameObject.name} 被选中，显示 UI");
            }
            else
            {
                //Destroy(currentCanvasInstance);
                currentCanvasInstance.SetActive(false);
                isSelected = false;
                Debug.Log($"{gameObject.name} 已取消选中，关闭 UI");
            }
        }



        public void OnDeselect()
        {
            isSelected = false;
            if (currentCanvasInstance.activeSelf)
            {
                currentCanvasInstance.SetActive(false);
                Debug.Log($"{gameObject.name} 被取消选择，隐藏 UI");
            }
        }
    }
}
