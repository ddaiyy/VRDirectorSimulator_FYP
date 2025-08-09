using UnityEngine;
using UnityEngine.UI;

public class VideoPanelController : MonoBehaviour
{
    public GameObject[] videoPanels;
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    private int currentIndex = 0;

    void Start()
    {
        // ��ʼ����ֻ��ʾ��һ����Ƶ
        for (int i = 0; i < videoPanels.Length; i++)
        {
            videoPanels[i].SetActive(i == 0);
        }

        prevButton.onClick.AddListener(ShowPrev);
        nextButton.onClick.AddListener(ShowNext);
        closeButton.onClick.AddListener(ClosePanel);

        closeButton.interactable = false; // ��ʼ���ɵ��
    }

    void ShowNext()
    {
        if (currentIndex < videoPanels.Length - 1)
        {
            videoPanels[currentIndex].SetActive(false);
            currentIndex++;
            videoPanels[currentIndex].SetActive(true);

            // ����Ѿ������һҳ������ Close ��ť����
            if (currentIndex == videoPanels.Length - 1)
            {
                closeButton.interactable = true;
            }
        }
    }

    void ShowPrev()
    {
        if (currentIndex > 0)
        {
            videoPanels[currentIndex].SetActive(false);
            currentIndex--;
            videoPanels[currentIndex].SetActive(true);

            // ֻҪ�������һҳ��Close ��ť�ͽ���
            if (currentIndex != videoPanels.Length - 1)
            {
                closeButton.interactable = false;
            }
        }
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
