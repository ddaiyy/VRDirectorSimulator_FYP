using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInitializer : MonoBehaviour
{

    public Transform spawnPoint; // 拖入一个生成点的空物体
    void Start()
    {
        int charID = GameManager.Instance.GetSelectedCharacterID();
        string action = GameManager.Instance.GetSelectedAction();
        string environment = GameManager.Instance.GetSelectedEnvironment();

        Debug.Log("Scene started with Character ID: " + charID + ", Action: " + action + ", Environment: " + environment);
        Debug.Log("Spawn character: " + charID);
        Debug.Log("Play action: " + action);
        Debug.Log("Enviornment: " + environment);

        // 你可以在这里实例化对应角色模型，并播放选择的动作
        // 安全检查
        if (charID >= 0 && charID < GameManager.Instance.characterPrefabs.Length)
        {
            // 实例化角色
            GameObject character = Instantiate(
                GameManager.Instance.characterPrefabs[charID],
                spawnPoint != null ? spawnPoint.position : Vector3.zero,
                Quaternion.identity
            );

            // 播放动画（假设你有 Animator 且动画名和动作名一致）
            Animator animator = character.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(action);
            }
        }
        else
        {
            Debug.LogWarning("Invalid character ID or missing prefab.");
        }
    }


}

