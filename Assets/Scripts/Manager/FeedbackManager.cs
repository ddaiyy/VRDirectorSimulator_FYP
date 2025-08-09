using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum MessageType
{
    Success,
    Warning,
    Error
}

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;

    [Header("UI References")]
    [SerializeField] private Canvas feedbackCanvas;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Image backgroundPanel;

    [Header("Settings")]
    [SerializeField] private float displayTime = 2f;
    [SerializeField] private float distanceFromCamera = 3f;

    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color warningColor = new Color(1f, 0.64f, 0f);
    [SerializeField] private Color errorColor = Color.red;

    private Queue<(string, MessageType)> messageQueue = new Queue<(string, MessageType)>();
    private bool isDisplaying = false;
    private Transform mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        if (feedbackText != null)
            feedbackText.text = "";

        if (feedbackCanvas != null)
            feedbackCanvas.gameObject.SetActive(false);

        // 找到玩家的主相机（XR 或普通摄像机）
        if (Camera.main != null)
            mainCamera = Camera.main.transform;
    }

    public void ShowMessage(string message, MessageType type)
    {
        messageQueue.Enqueue((message, type));

        if (!isDisplaying)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isDisplaying = true;

        if (feedbackCanvas != null)
            feedbackCanvas.gameObject.SetActive(true);

        while (messageQueue.Count > 0)
        {
            var (message, type) = messageQueue.Dequeue();
            Display(message, type);

            // 每次显示都更新位置到用户眼前
            UpdateCanvasPosition();

            yield return new WaitForSeconds(displayTime);
        }

        if (feedbackText != null)
            feedbackText.text = "";

        if (feedbackCanvas != null)
            feedbackCanvas.gameObject.SetActive(false);

        isDisplaying = false;
    }

    private void Display(string message, MessageType type)
    {
        if (feedbackText == null || backgroundPanel == null)
            return;

        feedbackText.text = message;

        switch (type)
        {
            case MessageType.Success:
                backgroundPanel.color = successColor * 0.5f;
                break;
            case MessageType.Warning:
                backgroundPanel.color = warningColor * 0.5f;
                break;
            case MessageType.Error:
                backgroundPanel.color = errorColor * 0.5f;
                break;
        }
    }

    private void UpdateCanvasPosition()
    {
        if (mainCamera == null) return;

        Vector3 targetPos = mainCamera.position + mainCamera.forward * distanceFromCamera;
        feedbackCanvas.transform.position = targetPos;

        // 让它面向玩家
        feedbackCanvas.transform.rotation = Quaternion.LookRotation(feedbackCanvas.transform.position - mainCamera.position);
    }
}
