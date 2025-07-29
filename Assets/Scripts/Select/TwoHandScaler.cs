using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TwoHandScaler : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    private List<IXRSelectInteractor> interactors = new List<IXRSelectInteractor>();

    private float initialDistance;
    private Vector3 initialScale;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        grabInteractable.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!interactors.Contains(args.interactorObject))
        {
            interactors.Add(args.interactorObject);
        }

        if (interactors.Count == 2)
        {
            initialDistance = GetInteractorDistance();
            initialScale = transform.localScale;  // 保存当前比例
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (interactors.Contains(args.interactorObject))
        {
            interactors.Remove(args.interactorObject);
        }

        // 💡 关键改动：如果还剩一只手在抓，就更新当前缩放为新的初始比例
        if (interactors.Count == 1)
        {
            initialScale = transform.localScale;
        }
    }

    void Update()
    {
        if (interactors.Count == 2)
        {
            float currentDistance = GetInteractorDistance();

            if (initialDistance > 0.001f)
            {
                float scaleFactor = currentDistance / initialDistance;
                transform.localScale = initialScale * scaleFactor;
            }

            // 可选旋转逻辑
            Vector3 dir = GetInteractorDirection();
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    float GetInteractorDistance()
    {
        return Vector3.Distance(
            interactors[0].transform.position,
            interactors[1].transform.position
        );
    }

    Vector3 GetInteractorDirection()
    {
        return interactors[1].transform.position - interactors[0].transform.position;
    }
}
