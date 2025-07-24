using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;

public class TwoHandGrabScalerAndRotator : MonoBehaviour
{
    private XRGrabInteractable grab;
    private List<IXRSelectInteractor> interactors = new List<IXRSelectInteractor>();

    private float initialDistance;
    private Vector3 initialScale;
    private Quaternion initialRotation;
    private Quaternion initialHandsRotation;

    private Vector3 lastScale;
    private bool wasTwoHanded = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.trackPosition = true;
            grab.trackRotation = true;
            grab.throwOnDetach = false;
            grab.useDynamicAttach = false;
            grab.attachTransform = null;  // 防止缩放被重置

            initialScale = transform.localScale;
            lastScale = transform.localScale;
            initialRotation = transform.rotation;
        }
    }

    void OnEnable()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnSelectEntered);
            grab.selectExited.AddListener(OnSelectExited);
        }
    }

    void OnDisable()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!interactors.Contains(args.interactorObject))
            interactors.Add(args.interactorObject);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (interactors.Contains(args.interactorObject))
            interactors.Remove(args.interactorObject);

        // 保存当前状态作为新的初始值
        initialScale = transform.localScale;
        lastScale = transform.localScale;
        initialRotation = transform.rotation;
        wasTwoHanded = false;

        // 关键：等一帧再设置缩放，防止系统重置
        StartCoroutine(ResetScaleNextFrame());
    }

    IEnumerator ResetScaleNextFrame()
    {
        yield return null;
        transform.localScale = lastScale;
    }

    void Update()
    {
        if (interactors.Count == 2)
        {
            if (!wasTwoHanded)
            {
                initialDistance = Vector3.Distance(GetPos(0), GetPos(1));
                initialScale = transform.localScale;
                initialRotation = transform.rotation;
                initialHandsRotation = GetRotationBetweenHands();
                wasTwoHanded = true;
            }

            // 缩放
            float currentDistance = Vector3.Distance(GetPos(0), GetPos(1));
            float scaleRatio = currentDistance / initialDistance;
            transform.localScale = initialScale * scaleRatio;
            lastScale = transform.localScale;

            // 旋转
            Quaternion currentHandsRotation = GetRotationBetweenHands();
            Quaternion rotationDelta = currentHandsRotation * Quaternion.Inverse(initialHandsRotation);
            transform.rotation = rotationDelta * initialRotation;
        }
    }

    void LateUpdate()
    {
        // 强制缩放锁定，防止 XR 系统偷偷修改
        if (lastScale != Vector3.zero)
        {
            transform.localScale = lastScale;
        }
    }

    private Vector3 GetPos(int index)
    {
        return (interactors[index] as MonoBehaviour).transform.position;
    }

    private Quaternion GetRotationBetweenHands()
    {
        Vector3 dir = GetPos(1) - GetPos(0);
        return Quaternion.LookRotation(dir != Vector3.zero ? dir : Vector3.forward);
    }
}
