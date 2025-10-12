using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance;

    public List<CardData> allCards = new List<CardData>();
    public List<CardData> specialCards = new List<CardData>();
    public List<CardData> neutralCards = new List<CardData>();
    public List<CardData> summonCards = new List<CardData>();
    // Dictionary: key = faction name, value = list of cards
    public Dictionary<string, List<CardData>> factionCards = new Dictionary<string, List<CardData>>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllCards();
    }

    void LoadAllCards()
    {
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Decks");

        foreach (var file in jsonFiles)
        {
            Debug.Log("Found Faction: " + file.name);

            CardCollection collection = JsonUtility.FromJson<CardCollection>(file.text);

            if (collection == null || string.IsNullOrEmpty(collection.faction))
            {
                Debug.LogWarning($"Failed to parse {file.name}");
                continue;
            }

            List<CardData> cardsForFaction = new List<CardData>();
            if (collection.leaders != null) cardsForFaction.AddRange(collection.leaders);
            if (collection.cards != null) cardsForFaction.AddRange(collection.cards);

            allCards.AddRange(cardsForFaction);

            // Sort into special / neutral / summon
            switch (collection.faction)
            {
                case "Special":
                    specialCards.AddRange(cardsForFaction);
                    break;
                case "Neutral":
                    neutralCards.AddRange(cardsForFaction);
                    break;
                case "Summon":
                    summonCards.AddRange(cardsForFaction);
                    break;
                default:
                    // normal faction
                    factionCards[collection.faction] = cardsForFaction;
                    break;
            }
        }

        // Add neutral cards to all factions
        foreach (var faction in factionCards.Keys)
        {
            factionCards[faction].AddRange(neutralCards);
        }

        // Sort decks
        SortFactionCards();

        Debug.Log($"Loaded cards for {factionCards.Count} factions");
    }

    public List<CardData> GetCardsByFaction(string faction)
    {
        if (factionCards.ContainsKey(faction))
            return factionCards[faction];
        return new List<CardData>();
    }

    void SortFactionCards()
    {
        string[] rangeOrder = { "melee", "agile", "ranged", "siege" };

        foreach (var faction in factionCards.Keys)
        {
            factionCards[faction].Sort((a, b) =>
            {
                // Special cards always last
                if (a.type == "special" && b.type != "special") return 1;
                if (b.type == "special" && a.type != "special") return -1;
                if (a.type == "special" && b.type == "special") return a.id.CompareTo(b.id);

                // Compare by strength
                int strengthCompare = a.strength.CompareTo(b.strength);
                if (strengthCompare != 0) return strengthCompare;

                // Compare by ID
                int idCompare = a.id.CompareTo(b.id);
                if (idCompare != 0) return idCompare;

                // Compare by range
                int aRangeIndex = System.Array.IndexOf(rangeOrder, a.range);
                int bRangeIndex = System.Array.IndexOf(rangeOrder, b.range);
                return aRangeIndex.CompareTo(bRangeIndex);
            });
        }
    }
}
