using UnityEngine;
using UnityEngine.UI;

public class BrightnessControllerInStartScene : MonoBehaviour
{
    public Light sceneLight;
    public Slider brightnessSlider;
    public float maxIntensity = 1.5f;

    void Start()
    {
        float savedValue = PlayerPrefs.GetFloat("SceneBrightness", 1f);
        ApplyBrightness(savedValue);

        brightnessSlider.value = savedValue;
        brightnessSlider.onValueChanged.AddListener(UpdateBrightness);
    }

    void UpdateBrightness(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat("SceneBrightness", value);
        PlayerPrefs.Save();
    }

    void ApplyBrightness(float value)
    {
        if (sceneLight != null)
            sceneLight.intensity = value * maxIntensity;
    }
}
