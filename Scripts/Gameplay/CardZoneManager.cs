using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles moving cards between zones (deck, hand, board, graveyard) and updating their UI.
/// </summary>
public class CardZoneManager
{
    private readonly BoardState state;
    private readonly BoardManager boardManager;
    private AbilityManager abilityManager;
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
    private void MoveCard(CardData card, List<CardData> fromZone, List<CardData> toZone)
    {
        if (fromZone == null || toZone == null)
        {
            Debug.LogError($"[CardZoneManager] Cannot move [{card.name}] as zone is null.");
            return;
        }

        // Reset any temporary modifications to the card
        card.strength = card.defaultStrength;
        card.range = card.defaultRange;

        if (fromZone.Contains(card))
            fromZone.Remove(card);

        if (!toZone.Contains(card))
            toZone.Add(card);

        CardSorter.Sort(toZone);

        PlayCardSFX(card, toZone);
        UpdateZoneUI(toZone);
        RefreshRowLayouts();
    }

    /// <summary>
    /// Updates the cards in a row to match the sorted zone list.
    /// </summary>
    private void UpdateZoneUI(List<CardData> zoneList)
    {
        Transform row = GetRowContainer(zoneList);

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
            boardManager.OpponentSiegeRow,
            boardManager.WeatherCardsContainer
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
        if (card.IsWeatherCard())
        {
            AddWeatherCard(card, isPlayer);
            return;
        }

        // Determine which zone the card is coming from
        List<CardData> fromZone = GetZoneContainingCard(card, isPlayer);
        List<CardData> targetRowList = GetTargetRowList(card, isPlayer);

        MoveCard(card, fromZone, targetRowList);

        Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} added [{card.name}] to {card.range} row");
    }

    /// <summary>
    /// Adds a weather card to the weather row (no duplicates).
    /// </summary>
    private void AddWeatherCard(CardData card, bool isPlayer)
    {
        // Clear weather removes all weather effects
        if (card.ability == CardDefs.Ability.Clear)
        {
            AddCardToGraveyard(card, isPlayer);
            return;
        }

        List<CardData> fromZone = GetZoneContainingCard(card, isPlayer);
        List<CardData> weatherList = state.weatherCards;

        // Prevent duplicates of the same weather type
        if (weatherList.Exists(c => c.ability == card.ability))
        {
            Debug.Log($"[CardZoneManager] Skipped adding duplicate weather: {card.name}");
            AddCardToGraveyard(card, isPlayer);
            return;
        }

        // If special weather, replace normal weather cards
        foreach (var weatherCard in weatherList.ToList())
        {
            // Normal weather card played, don't add if overridden by special weather
            if (card.ability == CardDefs.Ability.Frost && (weatherCard.ability == CardDefs.Ability.Nature || weatherCard.ability == CardDefs.Ability.WhiteFrost))
            {
                AddCardToGraveyard(card, isPlayer);
                return;
            }
            else if (card.ability == CardDefs.Ability.Fog && (weatherCard.ability == CardDefs.Ability.Storm || weatherCard.ability == CardDefs.Ability.WhiteFrost))
            {
                AddCardToGraveyard(card, isPlayer);
                return;
            }
            else if (card.ability == CardDefs.Ability.Rain && (weatherCard.ability == CardDefs.Ability.Storm || weatherCard.ability == CardDefs.Ability.Nature))
            { 
                AddCardToGraveyard(card, isPlayer);
                return;
            }

            // Special weather card played, remove normal weather cards it overrides
            else if (card.ability == CardDefs.Ability.Storm && (weatherCard.ability == CardDefs.Ability.Fog || weatherCard.ability == CardDefs.Ability.Rain))
                AddCardToGraveyard(weatherCard, isPlayer);

            else if (card.ability == CardDefs.Ability.Nature && (weatherCard.ability == CardDefs.Ability.Frost || weatherCard.ability == CardDefs.Ability.Rain))
                AddCardToGraveyard(weatherCard, isPlayer);

            else if (card.ability == CardDefs.Ability.WhiteFrost && (weatherCard.ability == CardDefs.Ability.Frost || weatherCard.ability == CardDefs.Ability.Fog))
                AddCardToGraveyard(weatherCard, isPlayer);
        }

        MoveCard(card, fromZone, weatherList);

        Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} added [{card.name}] to weather row");
    }

    /// <summary>
    /// Adds a card to a player's hand.
    /// </summary>
    public void AddCardToHand(CardData card, bool isPlayer)
    {
        // Determine which zone the card is coming from
        List<CardData> fromZone = GetZoneContainingCard(card, isPlayer);
        List<CardData> hand = isPlayer ? state.playerHand : state.opponentHand;

        MoveCard(card, fromZone, hand);

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

        if (card.ability == CardDefs.Ability.Avenger)
        {
            abilityManager.QueueAvenger(card, isPlayer);
            DiscardCard(card, isPlayer);
        }
        else if (card.type == CardDefs.Type.Standard)
        {
            MoveCard(card, fromZone, graveyard);
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

        // Destroy the card
        if (cardUIMap.TryGetValue(card, out var cardUI))
         {
            if (cardUI != null)
            {
                cardUI.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(cardUI.gameObject);
            }

             cardUIMap.Remove(card);
         }

        UpdateZoneUI(fromZone);
        RefreshRowLayouts();

        if (card.IsWeatherCard())
            Debug.Log($"[CardZoneManager] Discarded [{card.name}]");
        else
            Debug.Log($"[CardZoneManager] {(isPlayer ? "Player" : "Opponent")} discarded [{card.name}]");
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
                Debug.LogWarning($"[BoardManager] Unknown range {card.range} for card [{card.name}] � defaulting to melee row.");
                return addToPlayerRow ? state.playerMelee : state.opponentMelee;
        }
    }

    /// <summary>
    /// Gets the target row transform for the given row.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private Transform GetRowContainer(List<CardData> row)
    {
        if (row == state.playerHand) return boardManager.PlayerHandRow;
        if (row == state.playerMelee) return boardManager.PlayerMeleeRow;
        if (row == state.playerRanged) return boardManager.PlayerRangedRow;
        if (row == state.playerSiege) return boardManager.PlayerSiegeRow;
        if (row == state.playerDeck) return boardManager.PlayerDeckContainer;
        if (row == state.playerSummonDeck) return boardManager.PlayerSummonDeckContainer;
        if (row == state.playerGraveyard) return boardManager.PlayerGraveyardContainer;

        if (row == state.opponentHand) return boardManager.OpponentHandRow;
        if (row == state.opponentMelee) return boardManager.OpponentMeleeRow;
        if (row == state.opponentRanged) return boardManager.OpponentRangedRow;
        if (row == state.opponentSiege) return boardManager.OpponentSiegeRow;
        if (row == state.opponentDeck) return boardManager.OpponentDeckContainer;
        if (row == state.opponentSummonDeck) return boardManager.OpponentSummonDeckContainer;
        if (row == state.opponentGraveyard) return boardManager.OpponentGraveyardContainer;

        if (row == state.weatherCards) return boardManager.WeatherCardsContainer;

        Debug.LogWarning($"[BoardManager] Unknown row: {row}");
        return null;
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

        if (state.weatherCards.Contains(card)) return state.weatherCards;

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

    public void SetAbilityManager(AbilityManager abilityManager)
    {
        this.abilityManager = abilityManager;
    }
}
