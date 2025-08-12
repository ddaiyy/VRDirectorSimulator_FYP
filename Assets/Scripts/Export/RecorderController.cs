using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FfmpegUnity;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class RecorderController : MonoBehaviour
{
    public FfmpegCaptureCommand captureCommand;
    public Button startButton;
    public Canvas exportProgressCanvas;

    public TMP_Text progressText;//显示播放完成/正在进行的Text
    public Slider progressSlider;

    public TMP_Text progerssSliderTimeText;
    //public Button stopButton;

    public float recordDuration = 10f; // 录制时长（秒），可在Inspector设置
    public string outputFileName = "output.mp4"; // 输出文件名，可在Inspector设置

    private Coroutine recordCoroutine;

    private bool isCanvasActive = false;
    private float recordStartTime; 
    private bool isRecording = false;

    public Button closeCanvasButton;
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
    }

    
    void OnStartRecord()
    {
        recordDuration = TimelineManager.Instance.masterTrack.GetDuration();
        string fileName = outputFileName;
        if (!fileName.EndsWith(".mp4")) fileName += ".mp4";

#if UNITY_ANDROID && !UNITY_EDITOR
    string dirPath = "/sdcard/DCIM/Recordings/";
    Directory.CreateDirectory(dirPath);  // 确保目录存在
    string outputPath = dirPath + fileName;
    if (File.Exists(outputPath))
    {
        File.Delete(outputPath);
    }
#else
        string outputPath = Application.dataPath + "/" + fileName;
#endif
        // 设置ffmpeg参数，注意空格
        captureCommand.CaptureOptions = $" \"{outputPath}\"";
        
        if (recordCoroutine != null)
        {
            StopCoroutine(recordCoroutine);
        }
        
        recordCoroutine = StartCoroutine(RecordForSeconds(recordDuration));

    }

    IEnumerator RecordForSeconds(float seconds)
    {
        recordStartTime = Time.time;
        // 设置 slider 最大值
        progressSlider.maxValue = seconds;
        
        exportProgressCanvas.gameObject.SetActive(true);
        closeCanvasButton.gameObject.SetActive(false);
        isRecording = true;
        progressText.text = "Recording...";
        captureCommand.StartFfmpeg(); // 开始录制
        Debug.Log("开始录制");
        Debug.Log("PathInStreamingAssetsCopy: " + captureCommand.PathInStreamingAssetsCopy);
        Debug.Log("FFmpeg RunOptions: " + captureCommand.Options);
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

        // 如果录制完成
        if (!isRecording || elapsed >= recordDuration)
        {
            elapsed = recordDuration;
            progressSlider.value = recordDuration;
            progerssSliderTimeText.text = "100%";
            return;
        }

        // 正在录制时
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