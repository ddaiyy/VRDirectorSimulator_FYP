using UnityEngine;
using UnityEngine.UI;

public class ButtonAutoLinker : MonoBehaviour
{
    public CanvasAutoHideController canvasController;

    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
        {
            btn.onClick.AddListener(OnAnyButtonClicked);
        }
    }

    void OnAnyButtonClicked()
    {
        canvasController.FadeOutAndHide();
    }
}
