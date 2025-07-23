using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class TwoHandGrabScalerAndRotator : MonoBehaviour
{
    private XRGrabInteractable grab;
    private List<IXRSelectInteractor> interactors = new List<IXRSelectInteractor>();

    private float initialDistance;
    private Vector3 initialScale;
    private Quaternion initialRotation;
    private Quaternion initialHandsRotation;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
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

        if (interactors.Count == 2)
        {
            initialDistance = Vector3.Distance(GetPos(0), GetPos(1));
            initialScale = transform.localScale;

            initialHandsRotation = GetRotationBetweenHands();
            initialRotation = transform.rotation;
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        interactors.Remove(args.interactorObject);
    }

    void Update()
    {
        if (interactors.Count == 2)
        {
            // --- Ëõ·Å ---
            float currentDistance = Vector3.Distance(GetPos(0), GetPos(1));
            float scaleRatio = currentDistance / initialDistance;
            transform.localScale = initialScale * scaleRatio;

            // --- Ðý×ª ---
            Quaternion currentHandsRotation = GetRotationBetweenHands();
            Quaternion rotationDelta = currentHandsRotation * Quaternion.Inverse(initialHandsRotation);
            transform.rotation = rotationDelta * initialRotation;
        }
    }

    private Vector3 GetPos(int index)
    {
        return (interactors[index] as MonoBehaviour).transform.position;
    }

    private Quaternion GetRotationBetweenHands()
    {
        Vector3 handDir = GetPos(1) - GetPos(0);
        return Quaternion.LookRotation(handDir);
    }
}
