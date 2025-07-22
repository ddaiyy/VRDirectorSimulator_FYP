using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrajectoryVisualizer : MonoBehaviour
{
    public Transform cameraTransform; // Ҫ���ٵ������
    public float recordInterval = 0.1f; // ��¼�켣������룩
    public float trajectoryDuration = 5f; // ֹͣ�ƶ���켣ͣ��ʱ�䣨�룩
    public float fadeDuration = 1f; // �켣����ʱ�����룩

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
        // �� LineRenderer ������������Ϊ�ض��㣬���� UI ��
        gameObject.layer = LayerMask.NameToLayer("UI");

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.03f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // ���ﲻ��ʹ�� startColor/endColor����Ϊ�� gradient ������ɫ
        }

        SetupGradient();

        lastPosition = cameraTransform.position;
        lastMoveTime = Time.time;
    }

    private void SetupGradient()
    {
        gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = Color.yellow; // �켣β����ɫ���ϵ㣩
        colorKeys[0].time = 0f;
        colorKeys[1].color = Color.yellow; // �켣ͷ����ɫ���µ㣩
        colorKeys[1].time = 1f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 0f;  // �켣β��͸��
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f;  // �켣ͷ����͸��
        alphaKeys[1].time = 1f;

        gradient.SetKeys(colorKeys, alphaKeys);

        lineRenderer.colorGradient = gradient;
    }

    private void Update()
    {
        float moveThreshold = 0.01f;

        // �ж�������Ƿ��ƶ�������ֵ
        if (Vector3.Distance(cameraTransform.position, lastPosition) > moveThreshold)
        {
            RecordPosition(cameraTransform.position);
            lastMoveTime = Time.time;

            // ֹͣ�켣������������ڵ�����
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
                // ��֤�켣��������ʾ
                UpdateLineRendererPositions();
            }
        }

        lastPosition = cameraTransform.position;

        // ����Ƿ񳬹�ͣ��ʱ�䣬��������
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

        // ʹ�ù̶��Ľ��䣬���ⳬ�����ؼ�������
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        GradientColorKey[] colorKeys = new GradientColorKey[2];

        colorKeys[0] = new GradientColorKey(Color.yellow, 0f);
        colorKeys[1] = new GradientColorKey(Color.yellow, 1f);

        alphaKeys[0] = new GradientAlphaKey(0f, 0f); // β��͸��
        alphaKeys[1] = new GradientAlphaKey(1f, 1f); // ͷ����͸��

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }


    // ��ɾ���켣�㣬�γ�β����ʧЧ��
    private IEnumerator FadeOutTrajectoryByRemovingPoints()
    {
        float totalPoints = positions.Count;
        if (totalPoints == 0)
        {
            fadeCoroutine = null;
            yield break;
        }

        // ÿ�����ɾһ���㣬��֤fadeDuration��ɾ�����е�
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