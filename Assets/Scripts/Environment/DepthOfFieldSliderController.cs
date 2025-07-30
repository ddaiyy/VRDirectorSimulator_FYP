using UnityEngine;
using UnityEngine.UI;

public class DepthOfFieldFocusDistanceController : MonoBehaviour
{
    public Slider focusDistanceSlider;
    public Text focusDistanceValueText;

    private bool isUpdatingSlider = false; // ∑¿÷πSlider.value∏≥÷µ ±¥•∑¢OnValueChangedµº÷¬—≠ª∑

    private void Start()
    {
        if (focusDistanceSlider == null)
        {
            Debug.LogError("«Î∞Û∂®Ωπæ‡Slider£°");
            return;
        }

        focusDistanceSlider.minValue = 0.1f;
        focusDistanceSlider.maxValue = 10f;

        focusDistanceSlider.onValueChanged.AddListener(OnFocusDistanceChanged);

        SyncSliderWithSelectedCamera();
    }

    private void OnEnable()
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.OnSelectedCameraChanged += OnSelectedCameraChanged;
        }
    }

    private void OnDisable()
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.OnSelectedCameraChanged -= OnSelectedCameraChanged;
        }
    }

    private void OnSelectedCameraChanged(CameraController selectedCamera)
    {
        if (selectedCamera != null && focusDistanceSlider != null)
        {
            UpdateSliderAndText(selectedCamera.GetFocusDistance());
        }
    }

    private void OnFocusDistanceChanged(float value)
    {
        if (isUpdatingSlider) return; // ∑¿÷πµ›πÈµ˜”√

        var selectedCamera = CameraManager.Instance?.GetCurrentSelectedCamera();
        if (selectedCamera != null)
        {
            selectedCamera.SetFocusDistance(value);
        }
        UpdateValueText(value);
    }

    public void SyncSliderWithSelectedCamera()
    {
        var selectedCamera = CameraManager.Instance?.GetCurrentSelectedCamera();
        if (selectedCamera != null && focusDistanceSlider != null)
        {
            UpdateSliderAndText(selectedCamera.GetFocusDistance());
        }
    }

    private void UpdateSliderAndText(float value)
    {
        isUpdatingSlider = true;
        focusDistanceSlider.value = value;
        UpdateValueText(value);
        isUpdatingSlider = false;
    }

    private void UpdateValueText(float value)
    {
        if (focusDistanceValueText != null)
        {
            focusDistanceValueText.text = value.ToString("F2");
        }
    }
}
