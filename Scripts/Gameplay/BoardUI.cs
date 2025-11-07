using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles updating of board UI — scores, names, and deck/hand counts.
/// </summary>
public class BoardUI : MonoBehaviour
{
    [Header("Player UI")]
    public Image PlayerTurnIndicator;
    public Image PlayerWinningIndicator;
    public Image PlayerLife1;
    public Image PlayerLife2;
    public TextMeshProUGUI PlayerPassedText;
    public TextMeshProUGUI PlayerName;
    public TextMeshProUGUI PlayerFaction;
    public TextMeshProUGUI PlayerHandSize;
    public TextMeshProUGUI PlayerDeckSize;
    public TextMeshProUGUI PlayerTotalScore;
    public TextMeshProUGUI PlayerMeleeScore;
    public TextMeshProUGUI PlayerRangedScore;
    public TextMeshProUGUI PlayerSiegeScore;

    [Header("Opponent UI")]
    public Image OpponentTurnIndicator;
    public Image OpponentWinningIndicator;
    public Image OpponentLife1;
    public Image OpponentLife2;
    public TextMeshProUGUI OpponentPassedText;
    public TextMeshProUGUI OpponentName;
    public TextMeshProUGUI OpponentFaction;
    public TextMeshProUGUI OpponentHandSize;
    public TextMeshProUGUI OpponentDeckSize;
    public TextMeshProUGUI OpponentTotalScore;
    public TextMeshProUGUI OpponentMeleeScore;
    public TextMeshProUGUI OpponentRangedScore;
    public TextMeshProUGUI OpponentSiegeScore;

    [Header("Life Sprites")]
    public Sprite FullLifeSprite;
    public Sprite LostLifeSprite;

    [Header("Row Highlights")]
    public Image PlayerMeleeSpecialHighlight;
    public Image PlayerRangedSpecialHighlight;
    public Image PlayerSiegeSpecialHighlight;
    public Image PlayerMeleeRowHighlight;
    public Image PlayerRangedRowHighlight;
    public Image PlayerSiegeRowHighlight;

    [Header("Weather")]
    public Image FrostWeatherImage;
    public Image FogWeatherImage;
    public Image RainWeatherImage;
    public Image ClearWeatherImage;

    [Header("Banner")]
    public Image BannerImage;
    public TextMeshProUGUI BannerMessage;
    public Sprite[] BannerSprites;

    [Header("End Screen")]
    public GameObject EndScreenContainer;
    public Button EndScreenButton;
    public Image EndScreenImage;
    public TextMeshProUGUI PlayerEndScreenName;
    public TextMeshProUGUI PlayerRound1Score;
    public TextMeshProUGUI PlayerRound2Score;
    public TextMeshProUGUI PlayerRound3Score;
    public TextMeshProUGUI OpponentEndScreenName;
    public TextMeshProUGUI OpponentRound1Score;
    public TextMeshProUGUI OpponentRound2Score;
    public TextMeshProUGUI OpponentRound3Score;
    public Sprite[] EndScreenSprites;

    /// <summary>
    /// Updates all score and hand/deck counts based on current state.
    /// </summary>
    public void UpdateUI(BoardState state)
    {
        // -------------------------
        // Hands and deck counts
        // -------------------------
        PlayerHandSize.text = state.playerHand.Count.ToString();
        OpponentHandSize.text = state.opponentHand.Count.ToString();
        PlayerDeckSize.text = state.playerDeck.Count.ToString();
        OpponentDeckSize.text = state.opponentDeck.Count.ToString();

        // -------------------------
        // Scores
        // -------------------------
        PlayerMeleeScore.text = state.CalculateRowScore(state.playerMelee).ToString();
        PlayerRangedScore.text = state.CalculateRowScore(state.playerRanged).ToString();
        PlayerSiegeScore.text = state.CalculateRowScore(state.playerSiege).ToString();

        OpponentMeleeScore.text = state.CalculateRowScore(state.opponentMelee).ToString();
        OpponentRangedScore.text = state.CalculateRowScore(state.opponentRanged).ToString();
        OpponentSiegeScore.text = state.CalculateRowScore(state.opponentSiege).ToString();

        int playerScore = state.GetPlayerTotalScore();
        int opponentScore = state.GetOpponentTotalScore();
        PlayerTotalScore.text = playerScore.ToString();
        OpponentTotalScore.text = opponentScore.ToString();

        // -------------------------
        // Winnding indicator
        // -------------------------
        PlayerWinningIndicator.enabled = playerScore > opponentScore;
        OpponentWinningIndicator.enabled = opponentScore > playerScore;

        // -------------------------
        // Life indicators
        // -------------------------
        PlayerLife1.sprite = state.PlayerLife >= 1 ? FullLifeSprite : LostLifeSprite;
        PlayerLife2.sprite = state.PlayerLife >= 2 ? FullLifeSprite : LostLifeSprite;

        OpponentLife1.sprite = state.OpponentLife >= 1 ? FullLifeSprite : LostLifeSprite;
        OpponentLife2.sprite = state.OpponentLife >= 2 ? FullLifeSprite : LostLifeSprite;

        // -------------------------
        // Passed indicator
        // -------------------------
        PlayerPassedText.enabled = state.PlayerHasPassed;
        OpponentPassedText.enabled = state.OpponentHasPassed;

        // -------------------------
        // Turn indicator
        // -------------------------
        PlayerTurnIndicator.enabled = state.IsPlayerTurn;
        OpponentTurnIndicator.enabled = !state.IsPlayerTurn;
    }

    /// <summary>
    /// Sets up player and opponent names and factions at the start of a match.
    /// </summary>
    public void SetupPlayersInfo(string playerName, string playerFaction, string opponentName, string opponentFaction)
    {
        PlayerName.text = playerName;
        PlayerFaction.text = playerFaction;
        OpponentName.text = opponentName;
        OpponentFaction.text = opponentFaction;
    }

    /// <summary>
    /// Shows or hides row highlight for a given row.
    /// </summary>
    public void ShowRowHightlight(PlayerZone row, bool show)
    {
        switch (row)
        {
            case PlayerZone.MeleeSpecial:
                PlayerMeleeSpecialHighlight.enabled = show;
                break;
            case PlayerZone.RangedSpecial:
                PlayerRangedSpecialHighlight.enabled = show;
                break;
            case PlayerZone.SiegeSpecial:
                PlayerSiegeSpecialHighlight.enabled = show;
                break;
            case PlayerZone.MeleeRow:
                PlayerMeleeRowHighlight.enabled = show;
                break;
            case PlayerZone.RangedRow:
                PlayerRangedRowHighlight.enabled = show;
                break;
            case PlayerZone.SiegeRow:
                PlayerSiegeRowHighlight.enabled = show;
                break;
            default:
                Debug.LogWarning("[BoardUI] No highlight image for row: " + row);
                break;
        }
    }
    public void HideAllRowHighlights()
    {
        PlayerMeleeSpecialHighlight.enabled = false;
        PlayerRangedSpecialHighlight.enabled = false;
        PlayerSiegeSpecialHighlight.enabled = false;
        PlayerMeleeRowHighlight.enabled = false;
        PlayerRangedRowHighlight.enabled = false;
        PlayerSiegeRowHighlight.enabled = false;
    }

    /// <summary>
    /// Shows or hides weather effects on the board
    /// </summary>
    public void ShowWeather(string weatherAbility, bool show)
    {
        switch (weatherAbility)
        {
            case CardDefs.Ability.Clear:
                ClearWeatherImage.enabled = show;
                break;
            case CardDefs.Ability.Frost:
                FrostWeatherImage.enabled = show;
                break;
            case CardDefs.Ability.Fog:
                FogWeatherImage.enabled = show;
                break;
            case CardDefs.Ability.Rain:
                RainWeatherImage.enabled = show;
                break;
            case CardDefs.Ability.Storm:
                FogWeatherImage.enabled = show;
                RainWeatherImage.enabled = show;
                break;
            case CardDefs.Ability.Nature:
                FrostWeatherImage.enabled = show;
                RainWeatherImage.enabled = show;
                break;
            case CardDefs.Ability.WhiteFrost:
                FrostWeatherImage.enabled = show;
                FogWeatherImage.enabled = show;
                break;
            default:
                Debug.LogWarning("[BoardUI] Unsupported weather type: " + weatherAbility);
                break;
        }
    }
    public void HideAllWeather()
    {
        FrostWeatherImage.enabled = false;
        FogWeatherImage.enabled = false;
        RainWeatherImage.enabled = false;
        ClearWeatherImage.enabled = false;
    }

    /// <summary>
    /// Displays a banner with a message
    /// </summary>
    public void ShowBanner(Banner banner, string message)
    {
        BannerImage.enabled = true;
        BannerMessage.text = message;
        BannerImage.sprite = BannerSprites[(int)banner];
    }
    public void HideBanner()
    {
        BannerImage.enabled = false;
        BannerMessage.text = "";
    }

    /// <summary>
    /// Displays the end screen with details
    /// </summary>
    public void ShowEndScreen(EndScreen endScreen, BoardManager boardManager, BoardState state)
    {
        HideBanner();

        EndScreenContainer.SetActive(true);
        EndScreenButton.Select();
        EndScreenButton.onClick.RemoveAllListeners();
        EndScreenButton.onClick.AddListener(boardManager.QuitGame);
        EndScreenImage.sprite = EndScreenSprites[(int)endScreen];

        PlayerEndScreenName.text = PlayerName.text;
        PlayerRound1Score.text = GetRoundScore(state.PlayerRoundScores, 0);
        PlayerRound2Score.text = GetRoundScore(state.PlayerRoundScores, 1);
        PlayerRound3Score.text = GetRoundScore(state.PlayerRoundScores, 2);

        OpponentEndScreenName.text = OpponentName.text;
        OpponentRound1Score.text = GetRoundScore(state.OpponentRoundScores, 0);
        OpponentRound2Score.text = GetRoundScore(state.OpponentRoundScores, 1);
        OpponentRound3Score.text = GetRoundScore(state.OpponentRoundScores, 2);
    }
    public void HideEndScreen()
    {
        EndScreenContainer.SetActive(false);
    }
    private string GetRoundScore(List<int> scores, int index)
    {
        return (index < scores.Count) ? scores[index].ToString() : "-";
    }
}

public enum PlayerZone
{
    MeleeSpecial,
    RangedSpecial,
    SiegeSpecial,
    MeleeRow,
    RangedRow,
    SiegeRow
}

public enum Banner
{
    CoinPlayer,
    CoinOpponent,
    PlayerTurn,
    OpponentTurn,
    RoundPassed,
    RoundWin,
    RoundDraw,
    RoundLoss,
    NorthernRealms,
    Nilfgaard,
    Scoiatael,
    Monsters,
    Skellige
}

public enum EndScreen
{
    Win,
    Draw,
    Lose
}
