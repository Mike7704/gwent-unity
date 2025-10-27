using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all runtime data for the board — hands, rows, and score calculation.
/// </summary>
[System.Serializable]
public class BoardState
{
    // Game state
    public GamePhase CurrentPhase { get; set; }
    public bool PlayerCanAct { get; set; }
    public bool IsCardResolving { get; set; }
    public bool IsPlayerTurn { get; set; }
    public bool PlayerHasPassed { get; set; }
    public bool OpponentHasPassed { get; set; }
    public bool IsRoundOver { get; set; }
    public bool IsGameOver { get; set; }
    public int PlayerLife { get; set; }
    public int OpponentLife { get; set; }
    public List<int> PlayerRoundScores { get; private set; } = new();
    public List<int> OpponentRoundScores { get; private set; } = new();

    // Player and Opponent summon card decks
    public List<CardData> playerSummonDeck = new();
    public List<CardData> opponentSummonDeck = new();

    // Player and Opponent decks
    public List<CardData> playerDeck = new();
    public List<CardData> opponentDeck = new();

    // Player and Opponent hands
    public List<CardData> playerHand = new();
    public List<CardData> opponentHand = new();

    // Player and Opponent graveyards
    public List<CardData> playerGraveyard = new();
    public List<CardData> opponentGraveyard = new();

    // Player rows
    public List<CardData> playerMelee = new();
    public List<CardData> playerRanged = new();
    public List<CardData> playerSiege = new();

    // Opponent rows
    public List<CardData> opponentMelee = new();
    public List<CardData> opponentRanged = new();
    public List<CardData> opponentSiege = new();

    /// <summary>
    /// Calculates the score on a row.
    /// </summary>
    public int CalculateRowScore(List<CardData> row)
    {
        int total = 0;
        foreach (var card in row)
            total += card.strength;
        return total;
    }

    /// <summary>
    /// Calculates the total score for the player.
    /// </summary>
    public int GetPlayerTotalScore() 
    {
        return CalculateRowScore(playerMelee) + CalculateRowScore(playerRanged) + CalculateRowScore(playerSiege);
    }

    /// <summary>
    /// Calculates the total score for the opponent.
    /// </summary>
    public int GetOpponentTotalScore()
    {
        return CalculateRowScore(opponentMelee) + CalculateRowScore(opponentRanged) + CalculateRowScore(opponentSiege);
    }

    /// <summary>
    /// Record scores at the end of each round for the end screen
    /// </summary>
    public void RecordRoundScores()
    {
        PlayerRoundScores.Add(GetPlayerTotalScore());
        OpponentRoundScores.Add(GetOpponentTotalScore());
    }
}

public enum GamePhase
{
    Start,          // Decks loaded, hands drawn
    RedrawHand,     // Redraw phase
    PlayerTurn,     // Player plays card
    OpponentTurn,   // AI plays card
    ResolvingCard,  // Wait for card abilities (muster, decoy, etc.)
    RoundStart,     // Round start, determine who starts and update board
    RoundEnd,       // Round ended, calculate scores
    GameOver        // Someone lost all lives
}