using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GlobalVolumeControl : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("音乐音量")]
    public Slider musicSlider;
    public float musicMaxVolume = 1f;

    [Header("音效音量")]
    public Slider sfxSlider;
    public float sfxMaxVolume = 1f;

    void Start()
    {
        // 设置初始值（可以改为读取PlayerPrefs）
        musicSlider.value = 0.5f;
        sfxSlider.value = 0.5f;

        // 监听滑动条变化
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    void SetMusicVolume(float value)
    {
        SetMixerVolume("MusicVolume", value, musicMaxVolume);
    }

    void SetSFXVolume(float value)
    {
        SetMixerVolume("SFXVolume", value, sfxMaxVolume);
    }

    void SetMixerVolume(string parameterName, float sliderValue, float maxVolume)
    {
        float value = sliderValue * maxVolume;

        if (value <= 0.0001f)
        {
            audioMixer.SetFloat(parameterName, -80f); // 静音
            Debug.Log($"{parameterName} 设置为 静音");
        }
        else
        {
            float volumeDb = Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(parameterName, volumeDb);
            Debug.Log($"{parameterName}：滑动条值：{sliderValue} → dB音量：{volumeDb}");
        }
    }
}
