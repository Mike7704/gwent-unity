using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles moving cards between zones (deck, hand, board, graveyard) and updating their UI.
/// </summary>
public class CardZoneManager
{
    private readonly BoardState state;
    private readonly BoardManager board;
    private readonly Dictionary<CardData, CardUI> cardUIMap;

    public CardZoneManager(BoardState state, BoardManager board, Dictionary<CardData, CardUI> cardUIMap)
    {
        this.state = state;
        this.board = board;
        this.cardUIMap = cardUIMap;
    }

    /// <summary>
    /// Moves a card between zones.
    /// </summary>
    private void MoveCard(CardData card, List<CardData> fromZone, List<CardData> toZone, Transform targetRow)
    {
        if (fromZone == null || toZone == null)
        {
            Debug.LogError($"[CardZoneManager] Cannot move [{card.name}] as zone is null.");
            return;
        }

        if (fromZone.Contains(card))
            fromZone.Remove(card);

        if (!toZone.Contains(card))
            toZone.Add(card);

        CardSorter.Sort(toZone);

        UpdateZoneUI(toZone, targetRow);
        RefreshRowLayouts();
    }

    /// <summary>
    /// Updates the cards in a row to match the sorted zone list.
    /// </summary>
    private void UpdateZoneUI(List<CardData> zoneList, Transform row)
    {
        if (zoneList == null || row == null)
            return;

        for (int i = zoneList.Count - 1; i >= 0; i--)
        {
            var cardData = zoneList[i];
            if (cardUIMap.TryGetValue(cardData, out var cardUI))
            {
                cardUI.gameObject.SetActive(true);
                cardUI.transform.SetParent(row, false);
                cardUI.transform.SetAsLastSibling();
            }
        }
    }

    /// <summary>
    /// Refreshes the visual layout of cards on each row.
    /// </summary>
    private void RefreshRowLayouts()
    {
        Transform[] allRows =
        {
            board.PlayerHandRow,
            board.PlayerMeleeRow,
            board.PlayerRangedRow,
            board.PlayerSiegeRow,
            board.OpponentHandRow,
            board.OpponentMeleeRow,
            board.OpponentRangedRow,
            board.OpponentSiegeRow
        };

        foreach (var row in allRows)
        {
            if (row == null) continue;

            var layout = row.GetComponent<CardRowLayout>();
            if (layout != null)
                layout.LayoutCards();
        }
    }

    /// <summary>
    /// Displays a single card.
    /// </summary>
    public void DisplayCard(CardData card, Transform row)
    {
        if (card == null)
        {
            Debug.LogError("[CardZoneManager] Cannot display a null card.");
            return;
        }

        if (cardUIMap.TryGetValue(card, out var cardUI))
        {
            cardUI.gameObject.SetActive(true);
            if (row != null)
                cardUI.transform.SetParent(row, false);
        }
        else
        {
            Debug.LogWarning($"[CardZoneManager] Tried to display card with no UI: [{card.name}]");
        }
    }

    /// <summary>
    /// Adds a card to the board.
    /// </summary>
    public void AddCardToBoard(CardData card, bool isPlayer)
    {
        // Determine which zone the card is coming from
        List<CardData> fromZone = GetZoneContainingCard(card, isPlayer);
        List<CardData> targetRowList = GetTargetRowList(card, isPlayer);
        Transform targetRowTransform = GetTargetRowContainer(card, isPlayer);

        MoveCard(card, fromZone, targetRowList, targetRowTransform);

        Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} added [{card.name}] to {card.range} row");
    }

    /// <summary>
    /// Adds a card to a player's hand.
    /// </summary>
    public void AddCardToHand(CardData card, bool isPlayer)
    {
        // Determine which zone the card is coming from
        List<CardData> fromZone = GetZoneContainingCard(card, isPlayer);
        List<CardData> hand = isPlayer ? state.playerHand : state.opponentHand;
        Transform handTransform = isPlayer ? board.PlayerHandRow : board.OpponentHandRow;

        MoveCard(card, fromZone, hand, handTransform);

        Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} added [{card.name}] to hand");
    }

    /// <summary>
    /// Adds a card to the graveyard.
    /// </summary>
    public void AddCardToGraveyard(CardData card, bool isPlayer)
    {
        // Determine which zone the card is coming from
        List<CardData> fromZone = GetZoneContainingCard(card, isPlayer);
        List<CardData> graveyard = isPlayer ? state.playerGraveyard : state.opponentGraveyard;
        Transform graveyardTransform = isPlayer ? board.PlayerGraveyardContainer : board.OpponentGraveyardContainer;

        MoveCard(card, fromZone, graveyard, graveyardTransform);

        Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} sent [{card.name}] to graveyard");
    }

    /// <summary>
    /// Gets the target row transform based on card range and player/opponent.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public Transform GetTargetRowContainer(CardData card, bool isPlayer)
    {
        string range = card.range?.ToLower();

        switch (range)
        {
            case CardDefs.Range.Melee:
                return isPlayer ? board.PlayerMeleeRow : board.OpponentMeleeRow;
            case CardDefs.Range.Agile:
                return isPlayer ? board.PlayerMeleeRow : board.OpponentMeleeRow; // Melee as default for Agile for now
            case CardDefs.Range.Ranged:
                return isPlayer ? board.PlayerRangedRow : board.OpponentRangedRow;
            case CardDefs.Range.Siege:
                return isPlayer ? board.PlayerSiegeRow : board.OpponentSiegeRow;
            default:
                // Fallback if data is missing or invalid
                Debug.LogWarning($"[BoardManager] Unknown range '{range}' for card '[{card.name}]' — defaulting to melee row.");
                return isPlayer ? board.PlayerMeleeRow : board.OpponentMeleeRow;
        }
    }

    /// <summary>
    /// Gets the target list based on card range and player/opponent.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public List<CardData> GetTargetRowList(CardData card, bool isPlayer)
    {
        string range = card.range?.ToLower();

        switch (range)
        {
            case CardDefs.Range.Melee:
                return isPlayer ? state.playerMelee : state.opponentMelee;
            case CardDefs.Range.Agile:
                return isPlayer ? state.playerMelee : state.opponentMelee; // Melee as default for Agile for now
            case CardDefs.Range.Ranged:
                return isPlayer ? state.playerRanged : state.opponentRanged;
            case CardDefs.Range.Siege:
                return isPlayer ? state.playerSiege : state.opponentSiege;
            default:
                // Fallback if data is missing or invalid
                Debug.LogWarning($"[BoardManager] Unknown range '{range}' for card '[{card.name}]' — defaulting to melee row.");
                return isPlayer ? state.playerMelee : state.opponentMelee;
        }
    }

    /// <summary>
    /// Returns the zone (hand, deck, row, graveyard) that currently contains the card.
    /// </summary>
    private List<CardData> GetZoneContainingCard(CardData card, bool isPlayer)
    {
        if (isPlayer)
        {
            if (state.playerHand.Contains(card)) return state.playerHand;
            if (state.playerDeck.Contains(card)) return state.playerDeck;
            if (state.playerGraveyard.Contains(card)) return state.playerGraveyard;
            if (state.playerMelee.Contains(card)) return state.playerMelee;
            if (state.playerRanged.Contains(card)) return state.playerRanged;
            if (state.playerSiege.Contains(card)) return state.playerSiege;
        }
        else
        {
            if (state.opponentHand.Contains(card)) return state.opponentHand;
            if (state.opponentDeck.Contains(card)) return state.opponentDeck;
            if (state.opponentGraveyard.Contains(card)) return state.opponentGraveyard;
            if (state.opponentMelee.Contains(card)) return state.opponentMelee;
            if (state.opponentRanged.Contains(card)) return state.opponentRanged;
            if (state.opponentSiege.Contains(card)) return state.opponentSiege;
        }

        Debug.LogWarning($"[CardZoneManager] Could not find card [{card.name}] in any zone.");
        return null;
    }
}
