using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CharacterInfoDisplay : MonoBehaviour
{
    public GameObject infoCard; // 拖入角色下的 InfoCard 对象

    private void Start()
    {
        if (infoCard != null)
            infoCard.SetActive(false);
    }

    [ContextMenu("On Hover Enter")]
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (infoCard != null)
        {
            infoCard.SetActive(true);
            Debug.Log($"[CharacterInfoDisplay] Info card shown for: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("InfoCard reference not set.");
        }
    }

    [ContextMenu("On Hover Exit")]
    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (infoCard != null)
        {
            infoCard.SetActive(false);
            Debug.Log($"[CharacterInfoDisplay] Info card hidden for: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("InfoCard reference not set.");
        }
    }



}

