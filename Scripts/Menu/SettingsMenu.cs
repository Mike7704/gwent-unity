using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays and manages the Settings Menu UI.
/// </summary>
public class SettingsMenu : Singleton<SettingsMenu>
{
    [Header("UI References")]
    public Button ApplyButton;
    public Button BackButton;

    void Start()
    {
        ApplyButton.onClick.AddListener(ApplySettings);
        BackButton.onClick.AddListener(BackToMainMenu);
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
