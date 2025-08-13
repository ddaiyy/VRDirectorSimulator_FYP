using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FfmpegUnity;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit; // ✅ 新增，用于XR射线控制

public class RecorderController : MonoBehaviour
{
    public FfmpegCaptureCommand captureCommand;
    public Button startButton;
    public Canvas exportProgressCanvas;

    public TMP_Text progressText;//显示播放完成/正在进行的Text
    public Slider progressSlider;

    public TMP_Text progerssSliderTimeText;

    public float recordDuration = 10f; // 录制时长（秒），可在Inspector设置
    public string outputFileName = "output.mp4"; // 输出文件名，可在Inspector设置

    private Coroutine recordCoroutine;
    private float recordStartTime;
    private bool isRecording = false;

    public Button closeCanvasButton;

    // ✅ 新增：XR Ray Interactor 引用和层掩码
    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;
    public LayerMask normalMask;  // 场景交互的正常层
    public LayerMask uiOnlyMask;   // 只交互UI的层

    void Start()
    {
        captureCommand.ExecuteOnStart = false;
        startButton.onClick.AddListener(OnStartRecord);
        exportProgressCanvas.gameObject.SetActive(false);
        closeCanvasButton.onClick.AddListener(OnCloseCanvasButtonClicked);
    }

    private void OnCloseCanvasButtonClicked()
    {
        exportProgressCanvas.gameObject.SetActive(false);
        // ✅ 关闭UI后恢复正常场景交互
        SetRaycastMask(normalMask);
    }

    void OnStartRecord()
    {
        recordDuration = TimelineManager.Instance.masterTrack.GetDuration() + 2.0f;
        Debug.Log($"RecordDuration:{recordDuration}");
        string fileName = outputFileName;
        if (!fileName.EndsWith(".mp4")) fileName += ".mp4";

#if UNITY_ANDROID && !UNITY_EDITOR
        string outputPath = "/sdcard/DCIM/Recordings/" + fileName;
#else
        string outputPath = Application.dataPath + "/" + fileName;
#endif
        captureCommand.CaptureOptions = $"-y \"{outputPath}\"";

        if (recordCoroutine != null)
        {
            StopCoroutine(recordCoroutine);
        }

        recordCoroutine = StartCoroutine(RecordForSeconds(recordDuration));
    }

    IEnumerator RecordForSeconds(float seconds)
    {
        recordStartTime = Time.time;
        progressSlider.maxValue = seconds;

        exportProgressCanvas.gameObject.SetActive(true);
        closeCanvasButton.gameObject.SetActive(false);

        // ✅ 打开UI时限制射线只能打到UI
        SetRaycastMask(uiOnlyMask);

        isRecording = true;
        progressText.text = "Recording...";
        captureCommand.StartFfmpeg(); // 开始录制
        Debug.Log("开始录制");
        TimelineManager.Instance.masterTrack.Play();

        yield return new WaitForSeconds(seconds);

        captureCommand.StopFfmpeg(); // 停止录制
        progressText.text = "Finished!";
        isRecording = false;
        closeCanvasButton.gameObject.SetActive(true);
        Debug.Log("录制结束");

#if UNITY_ANDROID && !UNITY_EDITOR
        string fileName = outputFileName;
        if (!fileName.EndsWith(".mp4")) fileName += ".mp4";
        string outputPath = "/sdcard/DCIM/Recordings/" + fileName;
        RefreshMediaFile(outputPath);
#endif
    }

    private void Update()
    {
        if (isRecording)
        {
            RefreshExportCanvas();
        }
    }

    void RefreshExportCanvas()
    {
        float elapsed = Time.time - recordStartTime;

        if (!isRecording || elapsed >= recordDuration)
        {
            elapsed = recordDuration;
            progressSlider.value = recordDuration;
            progerssSliderTimeText.text = "100%";
            return;
        }

        progressSlider.value = Mathf.Clamp(elapsed, 0, recordDuration);
        float percent = (elapsed / recordDuration) * 100f;
        progerssSliderTimeText.text = $"{percent:F1}%";
    }

    void OnStopRecord()
    {
        if (recordCoroutine != null)
        {
            StopCoroutine(recordCoroutine);
            recordCoroutine = null;
        }

        captureCommand.StopFfmpeg();
        Debug.Log("手动停止录制");
    }

    // ✅ 新增：切换XR射线可交互的层
    void SetRaycastMask(LayerMask mask)
    {
        if (leftRay != null) leftRay.raycastMask = mask;
        if (rightRay != null) rightRay.raycastMask = mask;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public static void RefreshMediaFile(string filePath)
    {
        using (AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                using (AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
                {
                    mediaScanner.CallStatic("scanFile", context, new string[] { filePath }, null, null);
                }
            }
        }
    }
#endif
}
