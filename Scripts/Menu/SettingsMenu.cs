using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays and manages the Settings Menu UI.
/// </summary>
public class SettingsMenu : Singleton<SettingsMenu>
{
    [Header("UI References")]
    public Button ApplyButton;
    public Button BackButton;
    public TextMeshProUGUI descriptionText;

    void Start()
    {
        ApplyButton.onClick.AddListener(ApplySettings);
        BackButton.onClick.AddListener(BackToMainMenu);

        ClearDescription();
    }

    public void ShowDescription(string text)
    {
        if (descriptionText != null)
            descriptionText.text = text;
    }

    public void ClearDescription()
    {
        if (descriptionText != null)
            descriptionText.text = "";
    }

    /// <summary>
    /// Called when Apply button is clicked
    /// </summary>
    public void ApplySettings()
    {
        SettingsManager.Instance.SaveSettings();
    }

    /// <summary>
    /// Called when Back button is clicked
    /// </summary>
    public void BackToMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }

}
