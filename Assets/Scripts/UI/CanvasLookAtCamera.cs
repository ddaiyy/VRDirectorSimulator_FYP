using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CanvasLookAtCamera : MonoBehaviour
{
    public Transform mainCamera;  // VR主摄像机
    public float distance = 2.0f; // UI距离
    public float heightOffset = -0.2f; // 上下偏移

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // 目标位置 = 摄像机位置 + 前方 * 距离 + 高度偏移
        Vector3 targetPos = mainCamera.position + mainCamera.forward * distance;
        targetPos.y += heightOffset;

        transform.position = targetPos;

        // 让UI面向摄像机
        transform.LookAt(mainCamera);
        transform.Rotate(0, 180, 0); // 因为默认是背面朝摄像机
    }
}
