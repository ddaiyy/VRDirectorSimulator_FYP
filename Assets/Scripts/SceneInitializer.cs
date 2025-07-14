using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInitializer : MonoBehaviour
{

    public Transform spawnPoint; // ÍÏÈëÒ»¸öÉú³ÉµãµÄ¿ÕÎïÌå
    void Start()
    {
        int charID = GameManager.Instance.GetSelectedCharacterID();
        string action = GameManager.Instance.GetSelectedAction();
        string environment = GameManager.Instance.GetSelectedEnvironment();

        Debug.Log("Scene started with Character ID: " + charID + ", Action: " + action + ", Environment: " + environment);
        Debug.Log("Spawn character: " + charID);
        Debug.Log("Play action: " + action);
        Debug.Log("Enviornment: " + environment);

        // Äã¿ÉÒÔÔÚÕâÀïÊµÀý»¯¶ÔÓ¦½ÇÉ«Ä£ÐÍ£¬²¢²¥·ÅÑ¡ÔñµÄ¶¯×÷
        // °²È«¼ì²é
        if (charID >= 0 && charID < GameManager.Instance.characterPrefabs.Length)
        {
            // ÊµÀý»¯½ÇÉ«
            GameObject character = Instantiate(
                GameManager.Instance.characterPrefabs[charID],
                spawnPoint != null ? spawnPoint.position : Vector3.zero,
                Quaternion.identity
            );

            // ²¥·Å¶¯»­£¨¼ÙÉèÄãÓÐ Animator ÇÒ¶¯»­ÃûºÍ¶¯×÷ÃûÒ»ÖÂ£©
            Animator animator = character.GetComponent<Animator>();
            if (animator != null && !string.IsNullOrEmpty(action))
            {
                Debug.Log("Trying to set trigger: Do" + action);
                StartCoroutine(PlayActionLater(animator, action));


                animator.SetTrigger("Do" + action); // 比如 DoJump、DoWave
            }
            if (animator == null)
            {
                Debug.LogError("Animator component missing on character prefab!");
            }
            else if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("Animator Controller is not assigned!");
            }


        }
        else
        {
            Debug.LogWarning("Invalid character ID or missing prefab.");
        }
    }

    IEnumerator PlayActionLater(Animator animator, string action)
    {
        yield return null; // 等待一帧
        if (animator != null)
        {
            Debug.Log("Triggering animation: Do" + action);
            animator.SetTrigger("Do" + action);
        }
    }



}

