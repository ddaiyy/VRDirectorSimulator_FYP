using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrajectoryVisualizer : MonoBehaviour
{
    public Transform cameraTransform; // 要跟踪的摄像机
    public float recordInterval = 0.1f; // 记录轨迹间隔（秒）
    public float trajectoryDuration = 5f; // 停止移动后轨迹停留时间（秒）
    public float fadeDuration = 1f; // 轨迹淡出时长（秒）

    private LineRenderer lineRenderer;
    private List<Vector3> positions = new List<Vector3>();
    private float recordTimer = 0f;
    private float lastMoveTime = 0f;
    private Vector3 lastPosition;
    private Gradient gradient;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        // 把 LineRenderer 所在物体设置为特定层，比如 UI 层
        gameObject.layer = LayerMask.NameToLayer("UI");

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.03f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // 这里不再使用 startColor/endColor，因为用 gradient 控制颜色
        }

        SetupGradient();

        lastPosition = cameraTransform.position;
        lastMoveTime = Time.time;
    }

    private void SetupGradient()
    {
        gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = Color.yellow; // 轨迹尾部颜色（老点）
        colorKeys[0].time = 0f;
        colorKeys[1].color = Color.yellow; // 轨迹头部颜色（新点）
        colorKeys[1].time = 1f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 0f;  // 轨迹尾部透明
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f;  // 轨迹头部不透明
        alphaKeys[1].time = 1f;

        gradient.SetKeys(colorKeys, alphaKeys);

        lineRenderer.colorGradient = gradient;
    }

    private void Update()
    {
        float moveThreshold = 0.01f;

        // 判断摄像机是否移动超过阈值
        if (Vector3.Distance(cameraTransform.position, lastPosition) > moveThreshold)
        {
            RecordPosition(cameraTransform.position);
            lastMoveTime = Time.time;

            // 停止轨迹淡出（如果正在淡出）
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
                // 保证轨迹线完整显示
                UpdateLineRendererPositions();
            }
        }

        lastPosition = cameraTransform.position;

        // 检查是否超过停留时间，启动淡出
        if (Time.time - lastMoveTime > trajectoryDuration && positions.Count > 0 && fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(FadeOutTrajectoryByRemovingPoints());
        }
    }

    private void RecordPosition(Vector3 pos)
    {
        recordTimer += Time.deltaTime;
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            positions.Add(pos);
            UpdateLineRendererPositions();
        }
    }

    private void UpdateLineRendererPositions()
    {
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        // 使用固定的渐变，避免超出最大关键点限制
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        GradientColorKey[] colorKeys = new GradientColorKey[2];

        colorKeys[0] = new GradientColorKey(Color.yellow, 0f);
        colorKeys[1] = new GradientColorKey(Color.yellow, 1f);

        alphaKeys[0] = new GradientAlphaKey(0f, 0f); // 尾部透明
        alphaKeys[1] = new GradientAlphaKey(1f, 1f); // 头部不透明

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }


    // 逐步删除轨迹点，形成尾部消失效果
    private IEnumerator FadeOutTrajectoryByRemovingPoints()
    {
        float totalPoints = positions.Count;
        if (totalPoints == 0)
        {
            fadeCoroutine = null;
            yield break;
        }

        // 每隔多久删一个点，保证fadeDuration内删完所有点
        float interval = fadeDuration / totalPoints;

        while (positions.Count > 0)
        {
            positions.RemoveAt(0);
            UpdateLineRendererPositions();
            yield return new WaitForSeconds(interval);
        }

        fadeCoroutine = null;
    }
}