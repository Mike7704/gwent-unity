using TMPro;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Handles updating of board UI — scores, names, and deck/hand counts.
/// </summary>
public class BoardUI : MonoBehaviour
{
    [Header("Player UI")]
    public Image PlayerTurnIndicator;
    public Image PlayerLife1;
    public Image PlayerLife2;
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
    public Image OpponentLife1;
    public Image OpponentLife2;
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
        PlayerTotalScore.text = state.GetPlayerTotalScore().ToString();

        OpponentMeleeScore.text = state.CalculateRowScore(state.opponentMelee).ToString();
        OpponentRangedScore.text = state.CalculateRowScore(state.opponentRanged).ToString();
        OpponentSiegeScore.text = state.CalculateRowScore(state.opponentSiege).ToString();
        OpponentTotalScore.text = state.GetOpponentTotalScore().ToString();

        // -------------------------
        // Turn indicator
        // -------------------------
        PlayerTurnIndicator.enabled = state.IsPlayerTurn;
        OpponentTurnIndicator.enabled = !state.IsPlayerTurn;

        // -------------------------
        // Life indicators
        // -------------------------
        PlayerLife1.sprite = state.PlayerLife >= 1 ? FullLifeSprite : LostLifeSprite;
        PlayerLife2.sprite = state.PlayerLife >= 2 ? FullLifeSprite : LostLifeSprite;

        OpponentLife1.sprite = state.OpponentLife >= 1 ? FullLifeSprite : LostLifeSprite;
        OpponentLife2.sprite = state.OpponentLife >= 2 ? FullLifeSprite : LostLifeSprite;
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
}
