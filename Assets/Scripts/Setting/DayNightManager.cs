using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public Color dayAmbientColor = Color.white;
    public Color duskAmbientColor = new Color(1f, 0.6f, 0.3f);
    public Color nightAmbientColor = new Color(0.05f, 0.05f, 0.2f);
    public Color fantasyAmbientColor = new Color(0.6f, 0.3f, 1f);

    public Material daySkybox;
    public Material duskSkybox;
    public Material nightSkybox;
    public Material fantasySkybox;

    public enum TimeOfDay { Day = 0, Dusk = 1, Night = 2, Fantasy = 3 }

    void Awake()
    {
        // 场景加载时读取保存的时间段，默认白天
        int saved = PlayerPrefs.GetInt("UserTimeOfDay", 0);
        ApplyTimeOfDay((TimeOfDay)saved);
    }

    public void ApplyTimeOfDay(TimeOfDay time)
    {
        switch (time)
        {
            case TimeOfDay.Day:
                RenderSettings.ambientLight = dayAmbientColor;
                RenderSettings.skybox = daySkybox;
                break;
            case TimeOfDay.Dusk:
                RenderSettings.ambientLight = duskAmbientColor;
                RenderSettings.skybox = duskSkybox;
                break;
            case TimeOfDay.Night:
                RenderSettings.ambientLight = nightAmbientColor;
                RenderSettings.skybox = nightSkybox;
                break;
            case TimeOfDay.Fantasy:
                RenderSettings.ambientLight = fantasyAmbientColor;
                RenderSettings.skybox = fantasySkybox;
                break;
        }
    }

    // 让外部调用保存并应用
    public void SetTimeOfDay(TimeOfDay time)
    {
        PlayerPrefs.SetInt("UserTimeOfDay", (int)time);
        PlayerPrefs.Save();
        ApplyTimeOfDay(time);
    }
}
