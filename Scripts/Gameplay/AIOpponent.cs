using System.Collections;
using UnityEngine;

/// <summary>
/// Handles opponent logic for choosing and playing cards.
/// </summary>
public class AIOpponent
{
    private readonly BoardState state;
    private readonly BoardManager boardManager;

    public AIOpponent(BoardState state, BoardManager boardManager)
    {
        this.state = state;
        this.boardManager = boardManager;
    }

    /// <summary>
    /// Executes the opponent's turn.
    /// </summary>
    public IEnumerator PlayTurn()
    {
        if (state.IsPlayerTurn)
        {
            Debug.Log("[AIOpponent] It's the player's turn! Cannot play.");
            yield break;
        }

        if (state.opponentHand.Count == 0 || RandomUtils.GetRandom(0, 1) == 1)
        {
            Debug.Log("[AIOpponent] No cards left. Passing turn...");
            boardManager.PassRound(isPlayer: false);
            yield break;
        }

        int randomIndex = RandomUtils.GetRandom(0, state.opponentHand.Count - 1);
        CardData cardToPlay = state.opponentHand[randomIndex];

        boardManager.HandleCardPlayed(cardToPlay, isPlayer: false);
    }
}
