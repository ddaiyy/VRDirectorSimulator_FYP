using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject tooltipText;

    void Start()
    {
        // ????? TooltipText
        tooltipText = transform.Find("TooltipText")?.gameObject;
        if (tooltipText != null)
            tooltipText.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipText != null)
            tooltipText.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipText != null)
            tooltipText.SetActive(false);
    }
}
