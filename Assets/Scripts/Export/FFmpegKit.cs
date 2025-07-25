using UnityEngine;

public static class FFmpegKit
{
    public static void ExecuteAsync(string command)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var ffmpegKitClass = new AndroidJavaClass("com.arthenica.ffmpegkit.FFmpegKit"))
        {
            var callback = new FFmpegSessionCompleteCallback();
            ffmpegKitClass.CallStatic<AndroidJavaObject>("executeAsync", command, callback);
        }
#else
        Debug.LogWarning("FFmpegKit.ExecuteAsync 只在安卓设备上有效");
#endif
    }
}

// 必须和Java接口名完全一致
public class FFmpegSessionCompleteCallback : AndroidJavaProxy
{
    public FFmpegSessionCompleteCallback() : base("com.arthenica.ffmpegkit.FFmpegSessionCompleteCallback") { }

    // 方法签名必须和Java接口一致
    public void apply(AndroidJavaObject session)
    {
        Debug.Log("FFmpegKit命令执行完毕！");
        string logs = session.Call<string>("getAllLogsAsString");
        Debug.Log("FFmpeg日志: " + logs);
        string failStack = session.Call<string>("getFailStackTrace");
        Debug.Log("FFmpeg失败堆栈: " + failStack);
        var returnCode = session.Call<int>("getReturnCode");
        Debug.Log("FFmpeg ReturnCode"+returnCode);
        
    }
}