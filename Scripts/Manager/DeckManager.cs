using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedDeck
{
    public string faction;
    public List<int> cardIDs = new List<int>();
}

/// <summary>
/// Manages the player's active deck and handles saving/loading.
/// </summary>
public class DeckManager : Singleton<DeckManager>
{
    public string PlayerFaction { get; private set; }
    public string NPCFaction { get; private set; }
    public List<CardData> PlayerDeck { get; private set; } = new List<CardData>();
    public List<CardData> NPCDeck { get; private set; } = new List<CardData>();

    private const string SaveKey = "PlayerDeck";

    // Event hook for menus to refresh automatically
    public System.Action OnDeckChanged;

    /// <summary>
    /// Set the player faction.
    /// </summary>
    public void SetPlayerFaction(string faction)
    {
        // If no faction selected yet, or deck is empty — allow change
        if (string.IsNullOrEmpty(PlayerFaction) || PlayerDeck.Count == 0)
        {
            PlayerFaction = faction;
            OnDeckChanged?.Invoke();
        }
        else if (PlayerFaction != faction)
        {
            Debug.Log($"[DeckManager] Cannot change faction to {faction} — current deck has cards ({PlayerDeck.Count}).");
        }
    }

    /// <summary>
    /// Adds a card to the deck if it's valid and not already included.
    /// </summary>
    public void AddCard(CardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("[DeckManager] Tried to add a null card.");
            return;
        }

        // Restrict to faction (except neutral and special cards)
        if (!IsCardValidForPlayerDeck(card))
        {
            Debug.Log($"[DeckManager] Card {card.name} doesn't belong to faction {PlayerFaction}.");
            return;
        }

        if (!PlayerDeck.Contains(card))
        {
            PlayerDeck.Add(card);
            CardSorter.Sort(PlayerDeck);
            OnDeckChanged?.Invoke();
        }
    }

    /// <summary>
    /// Removes a card from the deck.
    /// </summary>
    public void RemoveCard(CardData card)
    {
        if (PlayerDeck.Remove(card))
        {
            CardSorter.Sort(PlayerDeck);
            OnDeckChanged?.Invoke();
        }
    }

    public bool ContainsCard(CardData card) => PlayerDeck.Contains(card);

    /// <summary>
    /// Clears all cards from the specified deck.
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="deckName"></param>
    public void ClearDeck(List<CardData> deck, string deckName = "Deck")
    {
        if (deck == null)
        {
            Debug.LogWarning($"[DeckManager] Tried to clear a null deck reference ({deckName}).");
            return;
        }

        PlayerFaction = String.Empty;
        deck.Clear();
        if (deck == PlayerDeck)
            OnDeckChanged?.Invoke();

        Debug.Log($"[DeckManager] {deckName} cleared.");
    }

    /// <summary>
    /// Generates a random deck with specified size.
    /// </summary>
    /// <param name="targetDeck"></param>
    /// <param name="deckSize"></param>
    public void RandomiseDeck(List<CardData> targetDeck, int deckSize)
    {
        var allFactions = CardDefs.GetPlayableFactions();
        if (allFactions == null || allFactions.Count == 0)
        {
            Debug.LogError("[DeckManager] No playable factions available for random deck.");
            return;
        }

        // Pick random faction
        string randomFaction = allFactions[RandomUtils.GetRandom(0, allFactions.Count - 1)];
        if (targetDeck == PlayerDeck)
            PlayerFaction = randomFaction;
        else if (targetDeck == NPCDeck)
            NPCFaction = randomFaction;

        // Get all cards valid for that faction
        var factionCards = CardDatabase.Instance.GetCardsByFaction(randomFaction);
        var neutralCards = CardDatabase.Instance.GetCardsByFaction(CardDefs.Faction.Neutral);
        var specialCards = CardDatabase.Instance.GetCardsByFaction(CardDefs.Faction.Special);

        // Merge pools
        List<CardData> validPool = new(factionCards);
        validPool.AddRange(neutralCards);
        validPool.AddRange(specialCards);

        if (validPool.Count == 0)
        {
            Debug.LogWarning($"[DeckManager] No valid cards found for faction {randomFaction}.");
            return;
        }

        // Randomly select cards
        targetDeck.Clear();
        deckSize = Mathf.Min(deckSize, validPool.Count);
        for (int i = 0; i < deckSize && validPool.Count > 0; i++)
        {
            var card = RandomUtils.Pick(validPool);
            targetDeck.Add(card);
            validPool.Remove(card); // Prevent duplicates
        }

        CardSorter.Sort(targetDeck);

        if (targetDeck == PlayerDeck)
            OnDeckChanged?.Invoke();

        Debug.Log($"[DeckManager] Generated a random {randomFaction} deck with {targetDeck.Count} cards.");
    }

    /// <summary>
    /// Save the current deck to PlayerPrefs as JSON.
    /// </summary>
    public void SaveDeck()
    {
        if (string.IsNullOrEmpty(PlayerFaction))
        {
            Debug.LogWarning("[DeckManager] Cannot save deck — faction not set.");
            return;
        }

        SavedDeck save = new()
        {
            faction = PlayerFaction
        };

        foreach (var card in PlayerDeck)
            save.cardIDs.Add(card.id);

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[DeckManager] Deck saved ({PlayerDeck.Count} cards) for faction {PlayerFaction}.");
    }

    /// <summary>
    /// Load the saved deck from PlayerPrefs.
    /// </summary>
    public void LoadDeck()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("[DeckManager] No saved deck found.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json)) return;

        SavedDeck saved = JsonUtility.FromJson<SavedDeck>(json);
        if (saved == null)
        {
            Debug.LogWarning("[DeckManager] Failed to parse saved deck.");
            return;
        }

        PlayerFaction = saved.faction;
        PlayerDeck.Clear();

        foreach (var id in saved.cardIDs)
        {
            var card = CardDatabase.Instance.GetCardById(id);
            if (card != null)
                PlayerDeck.Add(card);
        }

        CardSorter.Sort(PlayerDeck);
        OnDeckChanged?.Invoke();

        Debug.Log($"[DeckManager] Loaded deck ({PlayerDeck.Count} cards) for faction {PlayerFaction}.");
    }

    /// <summary>
    /// Checks whether a card belongs to the selected faction.
    /// </summary>
    public bool IsCardValidForPlayerDeck(CardData card)
    {
        return card.faction == PlayerFaction || PlayerDeck.Count == 0 || card.faction == CardDefs.Faction.Neutral || card.faction == CardDefs.Faction.Special;
    }
}
