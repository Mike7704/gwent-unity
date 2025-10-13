using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance;

    [Header("Card Lists")]
    public List<CardData> allCards = new List<CardData>();
    public List<CardData> specialCards = new List<CardData>();
    public List<CardData> neutralCards = new List<CardData>();
    public List<CardData> summonCards = new List<CardData>();

    // Dictionary: key = faction name, value = list of cards
    public Dictionary<string, List<CardData>> factionCards = new Dictionary<string, List<CardData>>();
    public Dictionary<string, List<CardData>> factionLeaders = new Dictionary<string, List<CardData>>();


    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllCards();

        // Initialise CardPool after cards are loaded
        if (CardPool.Instance != null)
            CardPool.Instance.PreloadAllCards(allCards);
        else
            Debug.LogWarning("CardPool instance not found.");
    }

    /// <summary>
    /// Called at the start of the game to load all card data from JSON files in Resources/Decks.
    /// </summary>
    void LoadAllCards()
    {
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Decks");

        foreach (var file in jsonFiles)
        {
            CardCollection collection = JsonUtility.FromJson<CardCollection>(file.text);

            if (collection == null || string.IsNullOrEmpty(collection.faction))
            {
                Debug.LogWarning($"Failed to parse {file.name}");
                continue;
            }

            Debug.Log($"Loaded deck: {collection.faction} ({file.name})");

            // Separate leaders from normal cards
            List<CardData> leaders = collection.leaders != null ? new List<CardData>(collection.leaders) : new List<CardData>();
            List<CardData> cards = collection.cards != null ? new List<CardData>(collection.cards) : new List<CardData>();

            // Fix asset paths for all cards
            foreach (var card in leaders)
            {
                if (!string.IsNullOrEmpty(card.imagePath)) card.imagePath = "Cards/" + card.imagePath;
                if (!string.IsNullOrEmpty(card.videoPath)) card.videoPath = "Cards/" + card.videoPath;
            }
            foreach (var card in cards)
            {
                if (!string.IsNullOrEmpty(card.imagePath)) card.imagePath = "Cards/" + card.imagePath;
                if (!string.IsNullOrEmpty(card.videoPath)) card.videoPath = "Cards/" + card.videoPath;
            }

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

        // Build master card list (no duplicates)
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

        Debug.Log($"Loaded {allCards.Count} cards for {factionCards.Count} factions");
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

    public CardData GetCardById(string id)
    {
        foreach (var card in allCards)
        {
            if (card.id.ToString() == id)
                return card;
        }
        return null;
    }

    /// <summary>
    /// Sort cards within each faction by type, strength, and range.
    /// </summary>
    private void SortFactionCards()
    {
        string[] rangeOrder = { "melee", "agile", "ranged", "siege" };

        foreach (var kvp in factionCards)
        {
            kvp.Value.Sort((a, b) =>
            {
                // Special cards last
                if (a.type == "special" && b.type != "special") return 1;
                if (b.type == "special" && a.type != "special") return -1;

                // Strength descending
                int strengthCompare = b.strength.CompareTo(a.strength);
                if (strengthCompare != 0) return strengthCompare;

                // Range order
                int aRangeIndex = System.Array.IndexOf(rangeOrder, a.range);
                int bRangeIndex = System.Array.IndexOf(rangeOrder, b.range);
                return aRangeIndex.CompareTo(bRangeIndex);
            });
        }
    }
}
