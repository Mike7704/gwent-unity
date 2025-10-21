using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's profile data.
/// </summary>
public class ProfileMenu : Singleton<ProfileMenu>
{
    [Header("Player Stats")]
    public TextMeshProUGUI WinsText;
    public TextMeshProUGUI DrawsText;
    public TextMeshProUGUI LossesText;
    public TextMeshProUGUI HighestScoreText;
    public TextMeshProUGUI UsernameText;
    public TextMeshProUGUI CoinBalanceText;

    [Header("Deck Completion")]
    public TextMeshProUGUI TotalCardsText;
    public TextMeshProUGUI SpecialCardsText;
    public TextMeshProUGUI NeutralCardsText;
    public TextMeshProUGUI NorthernRealmsCardsText;
    public TextMeshProUGUI NilfgaardCardsText;
    public TextMeshProUGUI ScoiataelCardsText;
    public TextMeshProUGUI MonstersCardsText;
    public TextMeshProUGUI SkelligeCardsText;

    [Header("Buttons")]
    public Button BackButton;

    void Start()
    {
        // Hook up button listeners
        BackButton.onClick.AddListener(BackToMainMenu);

        RefreshUI();
    }

    /// <summary>
    /// Refreshes the profile UI with the latest data from ProfileManager.
    /// </summary>
    public void RefreshUI()
    {
        var profile = ProfileManager.Instance;
        profile.LoadProfile();

        WinsText.text = $"Wins: {profile.Wins}";
        DrawsText.text = $"Draws: {profile.Draws}";
        LossesText.text = $"Losses: {profile.Losses}";
        HighestScoreText.text = $"Highest Score: {profile.HighestRoundScore}";
        UsernameText.text = profile.Username;
        CoinBalanceText.text = $"Coin Balance: {profile.CoinBalance}";

        TotalCardsText.text = $"Total Cards: {profile.TotalCardsUnlocked}";
        SpecialCardsText.text = $"Special: {profile.SpecialCardsUnlocked}";
        NeutralCardsText.text = $"Neutral: {profile.NeutralCardsUnlocked}";
        NorthernRealmsCardsText.text = $"Northern Realms: {profile.NorthernRealmsCardsUnlocked}";
        NilfgaardCardsText.text = $"Nilfgaard: {profile.NilfgaardCardsUnlocked}";
        ScoiataelCardsText.text = $"Scoita'tael: {profile.ScoiataelCardsUnlocked}";
        MonstersCardsText.text = $"Monsters: {profile.MonstersCardsUnlocked}";
        SkelligeCardsText.text = $"Skellige: {profile.SkelligeCardsUnlocked}";
    }

    /// <summary>
    /// Called when Back button is clicked
    /// </summary>
    public void BackToMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }
}
