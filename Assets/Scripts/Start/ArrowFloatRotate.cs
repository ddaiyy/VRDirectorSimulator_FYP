using UnityEngine;

public class ArrowFloatRotate : MonoBehaviour
{
    public float floatAmplitude = 0.2f;   // 上下浮动幅度（米）
    public float floatSpeed = 2f;         // 上下浮动速度
    public float rotationSpeed = 60f;     // 旋转速度（度/秒）

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;  // 记录初始局部位置
    }

    void Update()
    {
        // 上下浮动
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        // 持续绕Y轴旋转
        transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
