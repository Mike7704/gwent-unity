using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles moving cards between zones (deck, hand, board, graveyard) and updating their UI.
/// </summary>
public class CardZoneManager
{
    private readonly BoardState state;
    private readonly BoardManager boardManager;
    private readonly Dictionary<CardData, CardUI> cardUIMap;

    public CardZoneManager(BoardState state, BoardManager boardManager, Dictionary<CardData, CardUI> cardUIMap)
    {
        this.state = state;
        this.boardManager = boardManager;
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

        PlayCardSFX(card, toZone);
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
            boardManager.PlayerHandRow,
            boardManager.PlayerMeleeRow,
            boardManager.PlayerRangedRow,
            boardManager.PlayerSiegeRow,
            boardManager.OpponentHandRow,
            boardManager.OpponentMeleeRow,
            boardManager.OpponentRangedRow,
            boardManager.OpponentSiegeRow
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
        Transform handTransform = isPlayer ? boardManager.PlayerHandRow : boardManager.OpponentHandRow;

        MoveCard(card, fromZone, hand, handTransform);

        Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} added [{card.name}] to hand");
    }

    /// <summary>
    /// Adds a standard card to the graveyard.
    /// </summary>
    public void AddCardToGraveyard(CardData card, bool isPlayer)
    {
        // Determine which zone the card is coming from
        List<CardData> fromZone = GetZoneContainingCard(card, isPlayer);
        List<CardData> graveyard = isPlayer ? state.playerGraveyard : state.opponentGraveyard;
        Transform graveyardTransform = isPlayer ? boardManager.PlayerGraveyardContainer : boardManager.OpponentGraveyardContainer;

        if (card.type == CardDefs.Type.Standard)
        {
            MoveCard(card, fromZone, graveyard, graveyardTransform);
            Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} sent [{card.name}] to graveyard");
        }
        else
        {
            DiscardCard(card, isPlayer);
        }
    }

    /// <summary>
    /// Moves all cards on a row to the graveyard (at the end of a round)
    /// </summary>
    public void MoveRowToGraveyard(List<CardData> row, bool isPlayer)
    {
        // Make a copy so we can safely modify while iterating
        var cards = new List<CardData>(row);
        foreach (var card in cards)
            AddCardToGraveyard(card, isPlayer);
    }

    /// <summary>
    /// Completely removes a card from all zones and destroys its UI.
    /// </summary>
    private void DiscardCard(CardData card, bool isPlayer)
    {
        if (card == null) return;

        // Remove from any zone it's in
        List<CardData> fromZone = GetZoneContainingCard(card, isPlayer);
        fromZone?.Remove(card);

        // Destroy its UI if it exists
        if (cardUIMap.TryGetValue(card, out var cardUI))
        {
            if (cardUI != null)
                UnityEngine.Object.Destroy(cardUI.gameObject);

            cardUIMap.Remove(card);
        }

        Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} discarded [{card.name}]");
    }

    /// <summary>
    /// Gets the target row transform based on card range and player/opponent.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public Transform GetTargetRowContainer(CardData card, bool isPlayer)
    {
        bool isSpy = card.ability == CardDefs.Ability.Spy;
        bool addToPlayerRow = (isPlayer && !isSpy) || (!isPlayer && isSpy);

        switch (card.range)
        {
            case CardDefs.Range.Melee:
                return addToPlayerRow ? boardManager.PlayerMeleeRow : boardManager.OpponentMeleeRow;
            case CardDefs.Range.Agile:
                return addToPlayerRow ? boardManager.PlayerMeleeRow : boardManager.OpponentMeleeRow; // Melee as default for Agile for now
            case CardDefs.Range.Ranged:
                return addToPlayerRow ? boardManager.PlayerRangedRow : boardManager.OpponentRangedRow;
            case CardDefs.Range.Siege:
                return addToPlayerRow ? boardManager.PlayerSiegeRow : boardManager.OpponentSiegeRow;
            default:
                // Fallback if data is missing or invalid
                Debug.LogWarning($"[BoardManager] Unknown range {card.range} for card [{card.name}] — defaulting to melee row.");
                return addToPlayerRow ? boardManager.PlayerMeleeRow : boardManager.OpponentMeleeRow;
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
        bool isSpy = card.ability == CardDefs.Ability.Spy;
        bool addToPlayerRow = (isPlayer && !isSpy) || (!isPlayer && isSpy);

        switch (card.range)
        {
            case CardDefs.Range.Melee:
                return addToPlayerRow ? state.playerMelee : state.opponentMelee;
            case CardDefs.Range.Agile:
                return addToPlayerRow ? state.playerMelee : state.opponentMelee; // Melee as default for Agile for now
            case CardDefs.Range.Ranged:
                return addToPlayerRow ? state.playerRanged : state.opponentRanged;
            case CardDefs.Range.Siege:
                return addToPlayerRow ? state.playerSiege : state.opponentSiege;
            default:
                // Fallback if data is missing or invalid
                Debug.LogWarning($"[BoardManager] Unknown range {card.range} for card [{card.name}] — defaulting to melee row.");
                return addToPlayerRow ? state.playerMelee : state.opponentMelee;
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
            if (state.playerSummonDeck.Contains(card)) return state.playerSummonDeck;
        }
        else
        {
            if (state.opponentHand.Contains(card)) return state.opponentHand;
            if (state.opponentDeck.Contains(card)) return state.opponentDeck;
            if (state.opponentGraveyard.Contains(card)) return state.opponentGraveyard;
            if (state.opponentMelee.Contains(card)) return state.opponentMelee;
            if (state.opponentRanged.Contains(card)) return state.opponentRanged;
            if (state.opponentSiege.Contains(card)) return state.opponentSiege;
            if (state.opponentSummonDeck.Contains(card)) return state.opponentSummonDeck;
        }

        Debug.LogWarning($"[CardZoneManager] Could not find card [{card.name}] in any zone.");
        return null;
    }

    /// <summary>
    /// Plays sound effects based on the card and zone it is played to.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="zone"></param>
    private void PlayCardSFX(CardData card, List<CardData> zone)
    {
        if (zone == state.playerMelee || zone == state.opponentMelee)
        {
            if (card.type == CardDefs.Type.Hero)
                AudioSystem.Instance.PlaySFX(SFX.CardHero);
            else
                AudioSystem.Instance.PlaySFX(SFX.CardMelee);
        }
        else if (zone == state.playerRanged || zone == state.opponentRanged)
        {
            if (card.type == CardDefs.Type.Hero)
                AudioSystem.Instance.PlaySFX(SFX.CardHero);
            else
                AudioSystem.Instance.PlaySFX(SFX.CardRanged);
        }
        else if (zone == state.playerSiege || zone == state.opponentSiege)
        {
            if (card.type == CardDefs.Type.Hero)
                AudioSystem.Instance.PlaySFX(SFX.CardHero);
            else
                AudioSystem.Instance.PlaySFX(SFX.CardSiege);
        }
        else if (zone == state.playerDeck)
        {
            AudioSystem.Instance.PlaySFX(SFX.RedrawCard);
        }
    }
}
