using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loads and manages all card data from JSON files.
/// </summary>
public class CardDatabase : Singleton<CardDatabase>
{

    [Header("Card Lists")]
    public List<CardData> allCards = new List<CardData>();
    public List<CardData> specialCards = new List<CardData>();
    public List<CardData> neutralCards = new List<CardData>();
    public List<CardData> summonCards = new List<CardData>();

    // Dictionary: key = faction name, value = list of cards
    public Dictionary<string, List<CardData>> factionCards = new Dictionary<string, List<CardData>>();
    public Dictionary<string, List<CardData>> factionLeaders = new Dictionary<string, List<CardData>>();

    private Dictionary<int, CardData> cardIDLookup = new Dictionary<int, CardData>();

    /// <summary>
    /// Called at the start of the game to load all card data from JSON files in Resources/Decks.
    /// </summary>
    public void LoadAllCards()
    {
        allCards.Clear();
        specialCards.Clear();
        neutralCards.Clear();
        summonCards.Clear();
        factionCards.Clear();
        factionLeaders.Clear();

        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Decks");

        foreach (var file in jsonFiles)
        {
            if (file == null)
            {
                Debug.LogWarning("[CardDatabase] Found null JSON file in Decks folder.");
                continue;
            }

            CardCollection collection = JsonUtility.FromJson<CardCollection>(file.text);

            if (collection == null || string.IsNullOrEmpty(collection.faction))
            {
                Debug.LogWarning($"[CardDatabase] Failed to parse {file.name}");
                continue;
            }

            Debug.Log($"[CardDatabase] Loaded deck: {collection.faction} ({file.name})");

            // Separate leaders from normal cards
            List<CardData> leaders = collection.leaders != null ? new List<CardData>(collection.leaders) : new List<CardData>();
            List<CardData> cards = collection.cards != null ? new List<CardData>(collection.cards) : new List<CardData>();

            // Fix asset paths for all cards
            UpdateImageFilePath(leaders);
            UpdateImageFilePath(cards);

            // Add to global lists by faction type
            switch (collection.faction)
            {
                case "Special":
                    specialCards.AddRange(cards);
                    break;
                case "Neutral":
                    neutralCards.AddRange(cards);
                    break;
                case "Summon":
                    summonCards.AddRange(cards);
                    break;
                default:
                    // Normal faction: store cards separately from leaders
                    factionCards[collection.faction] = cards;

                    if (leaders.Count > 0)
                        factionLeaders[collection.faction] = leaders;

                    break;
            }
        }

        // Add shared neutral and special cards to each faction deck (only references)
        foreach (var faction in factionCards.Keys)
        {
            factionCards[faction].AddRange(neutralCards);
            factionCards[faction].AddRange(specialCards);
        }

        // Build master card list (unique set)
        HashSet<CardData> uniqueCards = new HashSet<CardData>();
        foreach (var factionDeck in factionCards.Values)
            foreach (var card in factionDeck)
                uniqueCards.Add(card);

        foreach (var leaderList in factionLeaders.Values)
            foreach (var card in leaderList)
                uniqueCards.Add(card);

        foreach (var card in neutralCards) uniqueCards.Add(card);
        foreach (var card in specialCards) uniqueCards.Add(card);
        foreach (var card in summonCards) uniqueCards.Add(card);

        allCards = new List<CardData>(uniqueCards);

        SortFactionCards();

        // Build fast lookup
        cardIDLookup.Clear();
        foreach (var card in allCards)
        {
            if (!cardIDLookup.ContainsKey(card.id))
                cardIDLookup.Add(card.id, card);
            else
                Debug.LogWarning($"[CardDatabase] Duplicate card ID detected: {card.id}");
        }


        Debug.Log($"[CardDatabase] Loaded {allCards.Count} cards across {factionCards.Count} factions.");
    }

    private void UpdateImageFilePath(List<CardData> list)
    {
        foreach (var card in list)
        {
            if (!string.IsNullOrEmpty(card.imagePath))
                card.imagePath = "Cards/" + card.imagePath;
            if (!string.IsNullOrEmpty(card.videoPath))
                card.videoPath = "Cards/" + card.videoPath;
        }
    }

    public List<CardData> GetCardsByFaction(string faction)
    {
        if (factionCards.TryGetValue(faction, out var cards))
            return cards;
        return new List<CardData>();
    }

    public List<CardData> GetLeadersByFaction(string faction)
    {
        if (factionLeaders.TryGetValue(faction, out var leaders))
            return leaders;
        return new List<CardData>();
    }

    public CardData GetCardById(int id)
    {
        if (cardIDLookup.TryGetValue(id, out var card))
            return card;

        Debug.LogWarning($"[CardDatabase] No card found with ID {id}");
        return null;
    }

    private void SortFactionCards()
    {
        foreach (var entry in factionCards)
            CardSorter.Sort(entry.Value);
    }
}
