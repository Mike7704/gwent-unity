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

    [Header("Banner")]
    public Image BannerImage;
    public TextMeshProUGUI BannerMessage;
    public Sprite[] BannerSprites;

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
