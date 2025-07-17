using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;
    public float zoomSpeed = 2f;
    public bool isRecording = false;
    public TimelineTrack timelineTrack;

    public void StartRecording() { /* TODO */ }
    public void StopRecording() { /* TODO */ }
    public void AddKeyframe(float time) { /* TODO */ }
    public void MoveTo(Vector3 position, Quaternion rotation, float fov) { /* TODO */ }
    public void ResetToInitial() { /* TODO */ }
    public void ExportCameraPath() { /* TODO */ }
} 