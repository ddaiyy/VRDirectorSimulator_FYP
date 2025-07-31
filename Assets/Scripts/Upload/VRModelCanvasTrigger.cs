using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MyGame.Selection;

public class VRModelCanvasTrigger : MonoBehaviour, ICustomSelectable
{
    public GameObject canvasPrefab;
    private GameObject canvasInstance;

    public void OnSelect()
    {
        if (canvasInstance == null && canvasPrefab != null)
        {
            canvasPrefab.SetActive(true);
            canvasInstance = Instantiate(canvasPrefab);
            canvasInstance.transform.SetParent(transform, false);

            // ��λ��ģ���Ϸ�
            canvasInstance.transform.localPosition = Vector3.up * 0.3f;
            canvasInstance.transform.localRotation = Quaternion.identity;
        }
        else if (canvasInstance != null)
        {
            canvasInstance.SetActive(!canvasInstance.activeSelf); // Toggle ��ʾ
        }
    }

    public void OnDeselect()
    {
        if (canvasPrefab != null)
        {
            canvasPrefab.SetActive(false);
            Debug.Log("ȡ��ѡ�У����� UI");
        }
    }

}
