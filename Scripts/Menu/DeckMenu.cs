using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Deck Building UI — displays faction cards, manages deck view, and handles card clicks.
/// </summary>
public class DeckMenu : Singleton<DeckMenu>
{
    [Header("UI References")]
    public Transform ContentPanel;  // Scroll View content
    public Button PlayerDeckButton; // Button to show the player deck
    public Button ClearDeckButton;
    public Button RandomiseDeckButton;
    public Button BackButton;

    [Header("Faction Buttons")]
    public Button NorthernRealmsButton;
    public Button NilfgaardButton;
    public Button ScoiataelButton;
    public Button MonstersButton;
    public Button SkelligeButton;

    private Dictionary<CardData, CardUI> visibleCardsDict = new();
    private string viewingDeck = "Player";

    void Start()
    {
        // Setup faction buttons
        NorthernRealmsButton.onClick.AddListener(() => DisplayFaction(CardDefs.Faction.NorthernRealms));
        NilfgaardButton.onClick.AddListener(() => DisplayFaction(CardDefs.Faction.Nilfgaard));
        ScoiataelButton.onClick.AddListener(() => DisplayFaction(CardDefs.Faction.Scoiatael));
        MonstersButton.onClick.AddListener(() => DisplayFaction(CardDefs.Faction.Monsters));
        SkelligeButton.onClick.AddListener(() => DisplayFaction(CardDefs.Faction.Skellige));

        // Setup other buttons
        PlayerDeckButton.onClick.AddListener(DisplayPlayerDeck);
        ClearDeckButton.onClick.AddListener(() => DeckManager.Instance.ClearDeck(DeckManager.Instance.PlayerDeck, "Player Deck"));
        RandomiseDeckButton.onClick.AddListener(() => DeckManager.Instance.RandomiseDeck(DeckManager.Instance.PlayerDeck, 25));
        BackButton.onClick.AddListener(BackToMainMenu);

        // Subscribe to deck changes
        DeckManager.Instance.OnDeckChanged += RefreshDeckView;

        DisplayPlayerDeck(); // Default view
    }

    // -------------------------
    // Display Logic
    // -------------------------

    /// <summary>
    /// Displays all cards for the given faction.
    /// </summary>
    private void DisplayFaction(string faction)
    {
        viewingDeck = faction;
        ClearScrollPane();

        // Make pool defensive for this scene so cards aren't lost
        CardManager.Instance.PurgeDestroyedCards();

        var cards = CardDatabase.Instance.GetCardsByFaction(faction);
        if (cards == null || cards.Count == 0)
        {
            Debug.LogWarning($"No cards found for faction: {faction}");
            return;
        }

        foreach (var data in cards)
        {
            CardUI cardUI = CardManager.Instance.CreateCard(data, ContentPanel);
            cardUI.ShowCardSelectedOverlay(DeckManager.Instance.PlayerDeck.Contains(data));
            visibleCardsDict[data] = cardUI;
        }

        // Force immediate layout so items appear correctly
        ForceLayout();

        Debug.Log($"[DeckMenu] Displaying {cards.Count} cards for faction: {faction}");
    }

    /// <summary>
    /// Displays the player’s current deck.
    /// </summary>
    private void DisplayPlayerDeck()
    {
        viewingDeck = "Player";
        ClearScrollPane();

        // Make pool defensive for this scene so cards aren't lost
        CardManager.Instance.PurgeDestroyedCards();

        var playerDeck = DeckManager.Instance.PlayerDeck;

        foreach (var data in playerDeck)
        {
            CardUI cardUI = CardManager.Instance.CreateCard(data, ContentPanel);
            cardUI.ShowCardSelectedOverlay(false);
            visibleCardsDict[data] = cardUI;
        }

        // Force immediate layout so items appear correctly
        ForceLayout();

        Debug.Log($"[DeckMenu] Displaying player deck ({playerDeck.Count} cards).");
    }

    // -------------------------
    // Card Interaction
    // -------------------------

    /// <summary>
    /// Called by CardUI when a card is clicked.
    /// Adds or removes it from the player’s deck.
    /// </summary>
    public void OnCardClicked(CardUI cardUI)
    {
        if (cardUI == null || cardUI.cardData == null)
            return;

        var data = cardUI.cardData;
        var deck = DeckManager.Instance;

        // Auto-select faction if not yet chosen
        if (string.IsNullOrEmpty(deck.PlayerFaction) &&
            data.faction != CardDefs.Faction.Neutral &&
            data.faction != CardDefs.Faction.Special)
        {
            deck.SetPlayerFaction(data.faction);
            Debug.Log($"[DeckMenu] Faction auto selected: {data.faction}");
        }

        // Prevent faction mismatches
        if (!deck.ContainsCard(data) && !DeckManager.Instance.IsCardValidForPlayerDeck(data))
        {
            Debug.Log($"[DeckMenu] Cannot add card: {data.name} (faction mismatch)");
            return;
        }

        // Add or remove from deck
        if (deck.ContainsCard(data))
        {
            deck.RemoveCard(data);
            cardUI.ShowCardSelectedOverlay(false);
            Debug.Log($"[DeckMenu] Removed {data.name} from deck");
        }
        else
        {
            deck.AddCard(data);
            cardUI.ShowCardSelectedOverlay(true);
            Debug.Log($"[DeckMenu] Added {data.name} to deck");
        }
    }

    // -------------------------
    // UI Helpers
    // -------------------------

    /// <summary>
    /// Refreshes the deck display by adding/removing cards as needed.
    /// </summary>
    private void RefreshDeckView()
    {
        Debug.Log("[DeckMenu] Refreshing deck view...");
        if (viewingDeck == "Player")
        {
            var deck = DeckManager.Instance.PlayerDeck;
            var cardsToRemove = new List<CardData>();

            // Remove cards no longer in deck
            foreach (var card in visibleCardsDict)
            {
                if (!deck.Contains(card.Key))
                {
                    cardsToRemove.Add(card.Key);
                }
            }
            foreach (var key in cardsToRemove)
            {
                CardManager.Instance.ReturnCard(visibleCardsDict[key]);
                visibleCardsDict.Remove(key);
            }

            // Add new cards
            foreach (var cardData in deck)
            {
                if (!visibleCardsDict.ContainsKey(cardData))
                {
                    CardUI cardUI = CardManager.Instance.CreateCard(cardData, ContentPanel);
                    visibleCardsDict[cardData] = cardUI;
                }
            }

            // Update overlays for cards
            foreach (var card in visibleCardsDict)
                card.Value.ShowCardSelectedOverlay(false);

            // Reorder cards to match sorted deck order
            for (int i = 0; i < deck.Count; i++)
            {
                var cardData = deck[i];
                if (visibleCardsDict.TryGetValue(cardData, out var cardUI))
                    cardUI.transform.SetSiblingIndex(i);
            }

            ForceLayout();
        }
        else
        {
            // Update faction view overlays
            foreach (var card in visibleCardsDict)
            {
                card.Value.ShowCardSelectedOverlay(DeckManager.Instance.PlayerDeck.Contains(card.Key));
            }
        }
    }

    /// <summary>
    /// Clears all cards from the scroll pane.
    /// </summary>
    private void ClearScrollPane()
    {
        foreach (var pair in visibleCardsDict)
            CardManager.Instance.ReturnCard(pair.Value);
        visibleCardsDict.Clear();
    }

    /// <summary>
    /// Forces the layout system to update immediately so UI elements are positioned correctly.
    /// </summary>
    private void ForceLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(ContentPanel as RectTransform);
    }

    // -------------------------
    // Change scene
    // -------------------------

    /// <summary>
    /// Called when Back button is clicked
    /// </summary>
    private void BackToMainMenu()
    {
        DeckManager.Instance.SaveDeck();
        ClearScrollPane();
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }
}
