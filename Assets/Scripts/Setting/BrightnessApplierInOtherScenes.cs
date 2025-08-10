using UnityEngine;

public class BrightnessApplierInOtherScenes : MonoBehaviour
{
    public float maxIntensity = 1f;
    public float transitionDuration = 1.0f; // 淡入持续时间

    private Light sceneLight;
    private float targetIntensity;
    private float transitionTimer = 0f;
    private float initialIntensity;

    void Start()
    {
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj != null)
        {
            sceneLight = lightObj.GetComponent<Light>();
            if (sceneLight != null)
            {
                float savedValue = PlayerPrefs.GetFloat("SceneBrightness", 1f);
                targetIntensity = savedValue * maxIntensity;

                // 保存当前亮度，准备插值
                initialIntensity = sceneLight.intensity;
            }
        }
        else
        {
            Debug.LogWarning("找不到名为 'Directional Light' 的光源");
        }
    }

    void Update()
    {
        if (sceneLight == null) return;

        // 平滑过渡到目标亮度
        if (transitionTimer < transitionDuration)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);
            sceneLight.intensity = Mathf.Lerp(initialIntensity, targetIntensity, t);
        }
    }
}
