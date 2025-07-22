using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MyGame.Selection;

public class XRSelectionForwarder : MonoBehaviour
{
    private XRRayInteractor rayInteractor;
    private ICustomSelectable currentSelectable;

    private void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
        if (rayInteractor == null)
        {
            Debug.LogError("需要 XRRayInteractor 组件！");
        }
    }

    private void OnEnable()
    {
        rayInteractor.selectEntered.AddListener(OnSelectEntered);
        rayInteractor.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        rayInteractor.selectEntered.RemoveListener(OnSelectEntered);
        rayInteractor.selectExited.RemoveListener(OnSelectExited);
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        var selectable = args.interactableObject.transform.GetComponent<ICustomSelectable>();
        if (selectable != null)
        {
            currentSelectable?.OnDeselect();
            currentSelectable = selectable;
            currentSelectable.OnSelect();
        }
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        var selectable = args.interactableObject.transform.GetComponent<ICustomSelectable>();
        if (selectable != null && currentSelectable == selectable)
        {
            currentSelectable.OnDeselect();
            currentSelectable = null;
        }
    }
}
