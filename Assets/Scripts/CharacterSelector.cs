using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    public int characterID = 0;

    [ContextMenu("Test Select")]
    public void OnSelected()
    {
        Debug.Log("Selected character ID: " + characterID);
        GameManager.Instance.SelectCharacter(characterID);
        Debug.Log("Selected character ID: " + characterID);
    }
}

