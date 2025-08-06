using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class CanvasSpawner : MonoBehaviour
{
    [Header("Canvas Ô¤ÖÆÌå")]
    public GameObject canvasPrefab;

    private static GameObject currentCanvasInstance;

    void Start()
    {
        // È·±£ÉãÏñ»ú´æÔÚ
        if (Camera.main == null)
        {
            Debug.LogError("Ã»ÓÐÕÒµ½Ö÷ÉãÏñ»ú£¡");
        }
    }

    // Õâ¸öº¯ÊýÔÚ°´Å¥µã»÷Ê±±»µ÷ÓÃ
    /*public void ToggleCanvas()
    {
        if (currentCanvasInstance == null)
        {
            SpawnCanvasInFrontOfUser();
        }
        else
        {
            Destroy(currentCanvasInstance);
            currentCanvasInstance = null;
        }
    }*/

    public void ToggleCanvas()
    {
        if (currentCanvasInstance != null)
        {
            Destroy(currentCanvasInstance);
            currentCanvasInstance = null;
        }

        SpawnCanvasInFrontOfUser();
    }


    void SpawnCanvasInFrontOfUser()
    {
        if (canvasPrefab == null || Camera.main == null) return;

        // »ñÈ¡ÉãÏñ»úÎ»ÖÃºÍ·½Ïò
        Transform cam = Camera.main.transform;

        Vector3 spawnPosition = cam.position + cam.forward * 5f + Vector3.up * -2f; // ÉÔÎ¢ÏÂÒÆÒ»µã
        Quaternion rotation = Quaternion.LookRotation(cam.forward);

        currentCanvasInstance = Instantiate(canvasPrefab, spawnPosition, rotation);
        currentCanvasInstance.SetActive(true);

        // ¿ÉÑ¡£ºÈÃ Canvas Ê¼ÖÕÕý¶ÔÍæ¼Ò£¨Ö»ÈÆ Y£©
        Vector3 lookPos = new Vector3(cam.position.x, currentCanvasInstance.transform.position.y, cam.position.z);
        currentCanvasInstance.transform.LookAt(lookPos);
        currentCanvasInstance.transform.Rotate(0, 180, 0); // ·´ÏòÊ¹ UI Õý¶Ô

        // ×Ô¶¯°ó¶¨ Close °´Å¥
        Button closeBtn = currentCanvasInstance.GetComponentInChildren<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(CloseCanvas);
        }
    }

    public void CloseCanvas()
    {
        if (currentCanvasInstance != null)
        {
            Destroy(currentCanvasInstance);
            currentCanvasInstance = null;
        }
    }

    [ContextMenu("²âÊÔ/´´½¨ Canvas")]
    void Debug_SpawnCanvas()
    {
        if (currentCanvasInstance == null)
            SpawnCanvasInFrontOfUser();
        else
            Debug.LogWarning("Canvas ÒÑ´æÔÚ£¬Ìø¹ý´´½¨");
    }

    [ContextMenu("²âÊÔ/Ïú»Ù Canvas")]
    void Debug_DestroyCanvas()
    {
        if (currentCanvasInstance != null)
            CloseCanvas();
        else
            Debug.LogWarning("Ã»ÓÐ Canvas ¿ÉÏú»Ù");
    }
}
