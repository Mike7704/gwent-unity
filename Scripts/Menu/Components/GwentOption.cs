using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public enum SettingType { Float, Int, Bool }

/// <summary>
/// Option UI component for adjusting a specific game setting.
/// </summary>
public class GwentOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI valueText;
    public Button leftButton;
    public Button rightButton;

    [Header("Configuration")]
    public string settingKey;
    public SettingType type;

    public float minValue = 0;
    public float maxValue = 10;
    public float step = 1;
    private float floatValue;
    private int intValue;
    private bool boolValue;

    private SettingsManager settings;

    void Start()
    {
        settings = SettingsManager.Instance;
        LoadFromSettings();

        leftButton.onClick.AddListener(OnDecrease);
        rightButton.onClick.AddListener(OnIncrease);
    }

    /// <summary>
    /// Loads the current setting value from SettingsManager and updates the display.
    /// </summary>
    public void LoadFromSettings()
    {
        if (settings == null) settings = SettingsManager.Instance;

        switch (type)
        {
            case SettingType.Float:
                floatValue = GetDisplayValue(settings.GetSettingValue<float>(settingKey));
                UpdateValueDisplay(((int)floatValue).ToString());
                break;

            case SettingType.Int:
                intValue = settings.GetSettingValue<int>(settingKey);
                UpdateValueDisplay(intValue.ToString());
                break;

            case SettingType.Bool:
                boolValue = settings.GetSettingValue<bool>(settingKey);
                UpdateValueDisplay(boolValue ? "On" : "Off");
                break;
        }
    }

    /// <summary>
    /// Increases the setting value and updates the display and SettingsManager.
    /// </summary>
    private void OnIncrease()
    {
        switch (type)
        {
            case SettingType.Float:
                floatValue = Mathf.Min(maxValue, floatValue + step);
                UpdateValueDisplay(((int)floatValue).ToString());
                settings.SetSetting(settingKey, GetActualValue(floatValue));
                break;

            case SettingType.Int:
                intValue = Mathf.Min((int)maxValue, intValue + (int)step);
                UpdateValueDisplay(intValue.ToString());
                settings.SetSetting(settingKey, intValue);
                break;

            case SettingType.Bool:
                boolValue = !boolValue;
                UpdateValueDisplay(boolValue ? "On" : "Off");
                settings.SetSetting(settingKey, boolValue);
                break;
        }
    }

    /// <summary>
    /// Decreases the setting value and updates the display and SettingsManager.
    /// </summary>
    private void OnDecrease()
    {
        switch (type)
        {
            case SettingType.Float:
                floatValue = Mathf.Max(minValue, floatValue - step);
                UpdateValueDisplay(((int)floatValue).ToString());
                settings.SetSetting(settingKey, GetActualValue(floatValue));
                break;

            case SettingType.Int:
                intValue = Mathf.Max((int)minValue, intValue - (int)step);
                UpdateValueDisplay(intValue.ToString());
                settings.SetSetting(settingKey, intValue);
                break;

            case SettingType.Bool:
                boolValue = !boolValue;
                UpdateValueDisplay(boolValue ? "On" : "Off");
                settings.SetSetting(settingKey, boolValue);
                break;
        }
    }

    /// <summary>
    /// Show description on pointer enter.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SettingsData.Descriptions.TryGetValue(settingKey, out string description))
        {
            SettingsMenu.Instance.ShowDescription(description);
        }
    }

    /// <summary>
    /// Hide description on pointer exit.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        SettingsMenu.Instance.ClearDescription();
    }

    /// <summary>
    /// Updates the displayed value text.
    /// </summary>
    /// <param name="newText"></param>
    private void UpdateValueDisplay(string newText)
    {
        if (valueText != null)
            valueText.text = newText;
    }

    /// <summary>
    /// Convert internal range (0–1) to displayed range (0–10) (for volume settings).
    /// </summary>
    /// <param name="actualValue"></param>
    /// <returns></returns>
    private float GetDisplayValue(float actualValue)
    {
        if (IsAudioSetting())
            return Mathf.Round(actualValue * 10f); // convert 0–1 to 0–10
        return actualValue;
    }

    /// <summary>
    /// Gets the actual setting value from the displayed value (for volume settings).
    /// </summary>
    /// <param name="displayValue"></param>
    /// <returns></returns>
    private float GetActualValue(float displayValue)
    {
        if (IsAudioSetting())
            return displayValue / 10f; // convert 0–10 to 0–1
        return displayValue;
    }

    /// <summary>
    /// Checks if the setting key corresponds to an audio setting.
    /// </summary>
    private bool IsAudioSetting()
    {
        return settingKey == SettingsData.MuteAudioKey ||
               settingKey == SettingsData.MusicVolumeKey ||
               settingKey == SettingsData.SFXVolumeKey ||
               settingKey == SettingsData.VoiceVolumeKey ||
               settingKey == SettingsData.VoicePercentageChanceKey;
    }
}