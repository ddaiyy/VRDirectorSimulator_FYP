using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayNightSettingUI : MonoBehaviour
{
    public DayNightManager dayNightManager;
    public TMP_Dropdown timeOfDayDropdown;

    void Start()
    {
        // 设置初始选项为保存值
        int saved = PlayerPrefs.GetInt("UserTimeOfDay", 0);
        timeOfDayDropdown.value = saved;

        timeOfDayDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    void OnDropdownChanged(int index)
    {
        dayNightManager.SetTimeOfDay((DayNightManager.TimeOfDay)index);
    }
}
