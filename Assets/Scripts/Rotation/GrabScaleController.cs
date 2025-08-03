using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabScaleController : MonoBehaviour
{
    [Header("��������")]
    public InputActionProperty leftXButtonAction; // X ��������С
    public InputActionProperty leftYButtonAction; // Y �����ƷŴ�

    [Header("���������")]
    public XRBaseInteractor rightHandGrab;

    [Header("���Ų���")]
    public float scaleSpeed = 0.5f;
    public float minScale = 0.1f;
    public float maxScale = 5f;

    private Transform grabbedObject;

    private void Update()
    {
        if (rightHandGrab.hasSelection)
        {
            grabbedObject = rightHandGrab.firstInteractableSelected.transform;

            bool isXPressed = leftXButtonAction.action.WasPressedThisFrame();
            bool isYPressed = leftYButtonAction.action.WasPressedThisFrame();

            float delta = 0f;
            if (isXPressed) delta -= scaleSpeed * Time.deltaTime;
            if (isYPressed) delta += scaleSpeed * Time.deltaTime;

            if (Mathf.Abs(delta) > 0.0001f)
            {
                Vector3 newScale = grabbedObject.localScale + Vector3.one * delta;

                float x = Mathf.Clamp(newScale.x, minScale, maxScale);
                float y = Mathf.Clamp(newScale.y, minScale, maxScale);
                float z = Mathf.Clamp(newScale.z, minScale, maxScale);
                grabbedObject.localScale = new Vector3(x, y, z);
            }
        }
    }

    // ��������Ĳ˵����Ҽ��˵��������ڲ��Ի��ʼ����
    [ContextMenu("Print Current Input Bindings")]
    public void PrintBindings()
    {
        Debug.Log("X Button Bound To: " + leftXButtonAction.action.bindings[0].effectivePath);
        Debug.Log("Y Button Bound To: " + leftYButtonAction.action.bindings[0].effectivePath);
    }
}
