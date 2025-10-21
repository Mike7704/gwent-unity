using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays and manages the Settings Menu UI.
/// </summary>
public class SettingsMenu : Singleton<SettingsMenu>
{
    [Header("Panels")]
    public Transform GameplayPanel;
    public Transform VideoPanel;
    public Transform AudioPanel;
    public Transform CreditsPanel;
    public Transform DefaultsPanel;

    [Header("Category Buttons")]
    public Button GameplayButton;
    public Button VideoButton;
    public Button AudioButton;
    public Button CreditsButton;
    public Button DefaultsButton;

    [Header("General UI")]
    public TextMeshProUGUI descriptionText;
    public Button ResetSettingsButton;
    public Button ResetProgressButton;
    public Button ApplyButton;
    public Button BackButton;

    private Transform currentPanel;

    void Start()
    {
        // Hook category buttons
        GameplayButton.onClick.AddListener(() => ShowPanel(GameplayPanel));
        VideoButton.onClick.AddListener(() => ShowPanel(VideoPanel));
        AudioButton.onClick.AddListener(() => ShowPanel(AudioPanel));
        CreditsButton.onClick.AddListener(() => ShowPanel(CreditsPanel));
        DefaultsButton.onClick.AddListener(() => ShowPanel(DefaultsPanel));

        // Hook general buttons
        ResetSettingsButton.onClick.AddListener(OnResetDefaults);
        //ResetProgressButton.onClick.AddListener(ResetProgressToDefaults);
        ApplyButton.onClick.AddListener(ApplySettings);
        BackButton.onClick.AddListener(BackToMainMenu);

        // Start on Gameplay tab by default
        ShowPanel(GameplayPanel);
    }

    /// <summary>
    /// Show the selected category panel, hide all others.
    /// </summary>
    private void ShowPanel(Transform panelToShow)
    {
        GameplayPanel.gameObject.SetActive(false);
        VideoPanel.gameObject.SetActive(false);
        AudioPanel.gameObject.SetActive(false);
        CreditsPanel.gameObject.SetActive(false);
        DefaultsPanel.gameObject.SetActive(false);

        panelToShow.gameObject.SetActive(true);
        currentPanel = panelToShow;

        // Refresh all options on that panel
        foreach (var option in panelToShow.GetComponentsInChildren<GwentOption>())
            option.LoadFromSettings();

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

    public void OnResetDefaults()
    {
        SettingsManager.Instance.ResetToDefaults();
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
