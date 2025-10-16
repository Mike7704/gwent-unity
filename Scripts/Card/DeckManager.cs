using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedDeck
{
    public string faction;
    public List<string> cardIDs = new List<string>();
}

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    public string selectedFaction;
    public List<CardData> playerDeck = new List<CardData>();

    private const string SaveKey = "PlayerDeck";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddCard(CardData data)
    {
        if (!playerDeck.Contains(data))
        {
            playerDeck.Add(data);
            CardSorter.Sort(playerDeck);
        }
    }

    public void RemoveCard(CardData data)
    {
        if (playerDeck.Contains(data))
        {
            playerDeck.Remove(data);
            CardSorter.Sort(playerDeck);
        }
    }

    public bool ContainsCard(CardData data)
    {
        return playerDeck.Contains(data);
    }

    public void ClearDeck()
    {
        playerDeck.Clear();
        selectedFaction = null;
    }

    /// <summary>
    /// Save the current deck to PlayerPrefs as JSON.
    /// </summary>
    public void SaveDeck()
    {
        SavedDeck save = new SavedDeck();
        save.faction = selectedFaction;

        foreach (var card in playerDeck)
            save.cardIDs.Add(card.id.ToString());

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"Deck saved ({playerDeck.Count} cards).");
    }

    /// <summary>
    /// Load the saved deck from PlayerPrefs.
    /// </summary>
    public void LoadDeck()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json))
            return;

        var saved = JsonUtility.FromJson<SavedDeck>(json);
        if (saved == null)
            return;

        selectedFaction = saved.faction;
        playerDeck.Clear();

        foreach (var id in saved.cardIDs)
        {
            var card = CardDatabase.Instance.GetCardById(id);
            if (card != null)
                playerDeck.Add(card);
        }

        Debug.Log($"Loaded saved player deck ({playerDeck.Count} cards) for faction {selectedFaction}");
    }
}
