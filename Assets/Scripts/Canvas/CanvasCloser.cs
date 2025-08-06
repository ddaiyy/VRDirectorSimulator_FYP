using UnityEngine;

public class CanvasCloser : MonoBehaviour
{
    // Ҫ�رյ� Canvas ������ Inspector ���Ͻ�����
    public GameObject canvasToClose;

    // �ڰ�ť�� OnClick() ����������
    public void CloseCanvas()
    {
        if (canvasToClose != null)
        {
            Destroy(canvasToClose); // ���� canvasToClose.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Canvas δ���ã�");
        }
    }
}
