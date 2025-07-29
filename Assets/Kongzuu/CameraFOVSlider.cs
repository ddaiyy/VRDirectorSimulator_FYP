using UnityEngine;
using UnityEngine.UI;

public class CameraFOVSlider : MonoBehaviour
{
    public Slider fovSlider;
    public Text fovValueText; // 可选，用于显示当前值

    private void Start()
    {
        // 初始化滑动条监听器
        if (fovSlider != null)
        {
            fovSlider.onValueChanged.AddListener(OnFOVChanged);
        }
    }

    private void OnFOVChanged(float value)
    {
        if (CameraManager.Instance != null)
        {
            CameraController selected = CameraManager.Instance.GetCurrentSelectedCamera();
            if (selected != null)
            {
                selected.SetFOV(value);
            }
        }

        if (fovValueText != null)
        {
            fovValueText.text = value.ToString("F0");
        }
    }
    public void SyncSlider(CameraController controller)
    {
        if (controller != null && fovSlider != null)
        {
            fovSlider.value = controller.GetFOV();
            if (fovValueText != null)
            {
                fovValueText.text = controller.GetFOV().ToString("F0");
            }
        }
    }

}
