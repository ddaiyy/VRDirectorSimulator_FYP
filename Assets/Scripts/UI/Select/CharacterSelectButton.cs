using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectButton : MonoBehaviour
{
    public CharacterActionController controllerForThisCharacter;

    public void OnCharacterSelected()
    {
        SelectedCharacterManager.CurrentSelectedCharacter = controllerForThisCharacter;
        Debug.Log($"选中了角色: {controllerForThisCharacter.gameObject.name}");
    }

    [ContextMenu("测试选中此角色")]
    private void TestSelectThisCharacter()
    {
        OnCharacterSelected();
    }
}
