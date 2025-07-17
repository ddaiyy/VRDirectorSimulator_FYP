using UnityEngine;

public interface IExportManager
{
    void ExportVideo(string outputPath);
    void ExportBlueprint(string outputPath);
    void ExportCameraPath(string outputPath);
    void ExportSceneConfiguration(string outputPath);
    void ExportTimelineData(string outputPath);
    void ExportAllData(string outputPath);
}

public class ExportManager : UnityEngine.MonoBehaviour, IExportManager
{
    public void ExportVideo(string outputPath) { /*TODO*/ }
    
    public void ExportBlueprint(string outputPath) { /*TODO*/ }
    
    //导出摄像机路径
    public void ExportCameraPath(string outputPath) { /*TODO*/ }
    
    //导出场景配置
    public void ExportSceneConfiguration(string outputPath) { /*TODO*/ }
    
    public void ExportTimelineData(string outputPath) { /*TODO*/ }
    
    public void ExportAllData(string outputPath) { /*TODO*/ }
} 