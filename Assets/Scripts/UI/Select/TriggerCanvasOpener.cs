using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TriggerCanvasOpener : MonoBehaviour
{
    private CharacterSelectionAndActionUI actionUI;

    void Awake()
    {
        actionUI = GetComponent<CharacterSelectionAndActionUI>();
    }

    public void OnActivated(ActivateEventArgs args)
    {
        if (actionUI != null)
            actionUI.OnCharacterButtonClicked();
    }

    void OnEnable()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.activated.AddListener(OnActivated);
    }

    void OnDisable()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.activated.RemoveListener(OnActivated);
    }
}
