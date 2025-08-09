using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FfmpegUnity;

public class RecorderController : MonoBehaviour
{
    public FfmpegCaptureCommand captureCommand;
    public UnityEngine.UI.Button startButton;
    //public Button stopButton;

    public float recordDuration = 10f; // 录制时长（秒），可在Inspector设置
    public string outputFileName = "output.mp4"; // 输出文件名，可在Inspector设置

    private Coroutine recordCoroutine;

    void Start()
    {
        captureCommand.ExecuteOnStart = false;
        startButton.onClick.AddListener(OnStartRecord);
    }

    void OnStartRecord()
    {
        float duration = TimelineManager.Instance.masterTrack.GetDuration();
        string fileName = outputFileName;
        if (!fileName.EndsWith(".mp4")) fileName += ".mp4";

#if UNITY_ANDROID && !UNITY_EDITOR
        string outputPath = "/sdcard/DCIM/Recordings/" + fileName;
#else
        string outputPath = Application.dataPath + "/" + fileName;
#endif
        // 设置ffmpeg参数，注意空格
        captureCommand.CaptureOptions = $" \"{outputPath}\"";
        
        if (recordCoroutine != null)
        {
            StopCoroutine(recordCoroutine);
        }
        
        recordCoroutine = StartCoroutine(RecordForSeconds(duration));

    }

    IEnumerator RecordForSeconds(float seconds)
    {
        captureCommand.StartFfmpeg(); // 开始录制
        Debug.Log("开始录制");
        TimelineManager.Instance.masterTrack.Play();
        yield return new WaitForSeconds(seconds);
        captureCommand.StopFfmpeg(); // 停止录制
        Debug.Log("录制结束");

#if UNITY_ANDROID && !UNITY_EDITOR
    string fileName = outputFileName;
    if (!fileName.EndsWith(".mp4")) fileName += ".mp4";
    string outputPath = "/sdcard/DCIM/Recordings/" + fileName;
    RefreshMediaFile(outputPath);
#endif
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