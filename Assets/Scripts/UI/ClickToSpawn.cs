using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ClickToSpawn : MonoBehaviour
{
    public GameObject prefabToSpawn;

    [ContextMenu("Test SpawnItem")]
    public void SpawnItem()
    {
        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}
