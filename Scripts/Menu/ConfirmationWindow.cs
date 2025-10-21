using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Displays a confirmation window with Yes/No options.
/// </summary>
public class ConfirmationWindow : Singleton<ConfirmationWindow>
{
    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button noButton;
    public Button yesButton;

    private Action onConfirm;

    void Start()
    {
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
        Hide();
    }

    /// <summary>
    /// Show the confirmation window with a message and an action to call if confirmed.
    /// </summary>
    public void Show(string title, string message, Action confirmAction)
    {
        titleText.text = title;
        messageText.text = message;
        onConfirm = confirmAction;

        panel.SetActive(true);
        noButton.Select(); // Focus on No button by default
    }

    private void OnYesClicked()
    {
        onConfirm?.Invoke();
        Hide();
    }

    private void OnNoClicked()
    {
        Hide();
    }

    public void Hide()
    {
        panel.SetActive(false);
        onConfirm = null;
    }
}
