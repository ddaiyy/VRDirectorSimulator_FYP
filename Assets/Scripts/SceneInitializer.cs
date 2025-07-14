using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInitializer : MonoBehaviour
{

    public Transform spawnPoint; // ����һ�����ɵ�Ŀ�����
    void Start()
    {
        int charID = GameManager.Instance.GetSelectedCharacterID();
        string action = GameManager.Instance.GetSelectedAction();
        string environment = GameManager.Instance.GetSelectedEnvironment();

        Debug.Log("Scene started with Character ID: " + charID + ", Action: " + action + ", Environment: " + environment);
        Debug.Log("Spawn character: " + charID);
        Debug.Log("Play action: " + action);
        Debug.Log("Enviornment: " + environment);

        // �����������ʵ������Ӧ��ɫģ�ͣ�������ѡ��Ķ���
        // ��ȫ���
        if (charID >= 0 && charID < GameManager.Instance.characterPrefabs.Length)
        {
            // ʵ������ɫ
            GameObject character = Instantiate(
                GameManager.Instance.characterPrefabs[charID],
                spawnPoint != null ? spawnPoint.position : Vector3.zero,
                Quaternion.identity
            );

            // ���Ŷ������������� Animator �Ҷ������Ͷ�����һ�£�
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

