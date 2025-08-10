using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoPanelController : MonoBehaviour
{
    public GameObject[] videoPanels;
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;
    public TMP_Text pageIndicator;

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

        // ��ʼ״̬
        closeButton.interactable = false;
        prevButton.interactable = false;  // ��һҳ���ܵ���һҳ
        nextButton.interactable = videoPanels.Length > 1; // ���ֻ��1ҳ�ͽ���Next

        UpdatePageIndicator(); // ��ʼ����ҳ��ʾ
    }

    void ShowNext()
    {
        if (currentIndex < videoPanels.Length - 1)
        {
            videoPanels[currentIndex].SetActive(false);
            currentIndex++;
            videoPanels[currentIndex].SetActive(true);

            UpdateButtonStates();
        }
    }

    void ShowPrev()
    {
        if (currentIndex > 0)
        {
            videoPanels[currentIndex].SetActive(false);
            currentIndex--;
            videoPanels[currentIndex].SetActive(true);

            UpdateButtonStates();
        }
    }

    void UpdateButtonStates()
    {
        // ��һҳ����Prev
        prevButton.interactable = currentIndex > 0;

        // ���һҳ����Next
        nextButton.interactable = currentIndex < videoPanels.Length - 1;

        // Close��ť��ֻ�����һҳ�ſ���
        closeButton.interactable = currentIndex == videoPanels.Length - 1;

        UpdatePageIndicator();
    }

    void UpdatePageIndicator()
    {
        if (pageIndicator != null)
        {
            pageIndicator.text = $"{currentIndex + 1} / {videoPanels.Length}";
        }
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
