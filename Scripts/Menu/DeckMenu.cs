using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Deck Building UI — displays faction cards, manages deck view, and handles card clicks.
/// </summary>
public class DeckMenu : Singleton<DeckMenu>
{
    [Header("UI References")]
    public Transform DeckCardsContentPanel;  // Deck scroll view content
    public Button BackButton;

    [Header("Leader Container")]
    public Transform LeaderContainerPanel;
    public Transform LeaderCardContainer;
    public Button PrevLeaderButton;
    public Button NextLeaderButton;

    [Header("Deck Buttons Container")]
    public Transform DeckButtonsContainerPanel;
    public Button ClearDeckButton;
    public Button RandomiseDeckButton;

    [Header("Faction Buttons")]
    public Button NorthernRealmsButton;
    public Button NilfgaardButton;
    public Button ScoiataelButton;
    public Button MonstersButton;
    public Button SkelligeButton;
    public Button PlayerDeckButton; // Button to show the player deck

    // Leader cards
    private List<CardData> currentLeaders = new();
    private int currentLeaderIndex = 0;
    private CardUI currentLeaderUI;

    // Visible deck cards in the scroll pane
    private readonly Dictionary<CardData, CardUI> visibleCardsDict = new();
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
        PrevLeaderButton.onClick.AddListener(ShowPreviousLeader);
        NextLeaderButton.onClick.AddListener(ShowNextLeader);
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
        UpdateUIVisiblity();
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
            CardUI cardUI = CardManager.Instance.CreateCard(data, cropped: false, DeckCardsContentPanel);
            cardUI.OnCardClicked += HandleCardInteraction;
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
        UpdateUIVisiblity();
        ClearScrollPane();

        // Make pool defensive for this scene so cards aren't lost
        CardManager.Instance.PurgeDestroyedCards();

        // Show leader card
        DisplayLeader();

        var playerDeck = DeckManager.Instance.PlayerDeck;

        foreach (var data in playerDeck)
        {
            CardUI cardUI = CardManager.Instance.CreateCard(data, cropped: false, DeckCardsContentPanel);
            cardUI.OnCardClicked += HandleCardInteraction;
            cardUI.ShowCardSelectedOverlay(false);
            visibleCardsDict[data] = cardUI;
        }

        // Force immediate layout so items appear correctly
        ForceLayout();

        AudioSystem.Instance.PlaySFX(SFX.OpenDeck);

        Debug.Log($"[DeckMenu] Displaying player deck ({playerDeck.Count} cards).");
    }

    /// <summary>
    /// Displays the current leader card for the selected faction.
    /// </summary>
    private void DisplayLeader()
    {
        // Clear old leader UI if exists
        if (currentLeaderUI != null)
        {
            CardManager.Instance.ReturnCard(currentLeaderUI);
            currentLeaderUI = null;
        }

        // Ensure faction is selected before showing leaders
        string faction = DeckManager.Instance.PlayerFaction;
        if (string.IsNullOrEmpty(faction))
        {
            Debug.Log("[DeckMenu] No faction selected — cannot display leaders.");
            return;
        }

        // Get all leader cards for this faction
        currentLeaders = CardDatabase.Instance.GetLeadersByFaction(faction);
        if (currentLeaders == null || currentLeaders.Count == 0)
        {
            Debug.LogWarning($"[DeckMenu] No leaders found for faction: {faction}");
            return;
        }

        // Make sure index stays valid
        if (currentLeaderIndex >= currentLeaders.Count)
            currentLeaderIndex = 0;

        // Create the current leader card UI
        var leaderData = currentLeaders[currentLeaderIndex];
        currentLeaderUI = CardManager.Instance.CreateCard(leaderData, cropped: false, LeaderCardContainer);

        // Update leader in DeckManager
        DeckManager.Instance.SetPlayerLeader(leaderData);
    }

    /// <summary>
    /// Displays the next leader card in the list.
    /// </summary>
    private void ShowNextLeader()
    {
        if (currentLeaders.Count == 0) return;
        currentLeaderIndex++;
        if (currentLeaderIndex > currentLeaders.Count - 1)
            currentLeaderIndex = 0;
        DisplayLeader();
    }

    /// <summary>
    /// Displays the previous leader card in the list.
    /// </summary>
    private void ShowPreviousLeader()
    {
        if (currentLeaders.Count == 0) return;
        currentLeaderIndex--;
        if (currentLeaderIndex < 0)
            currentLeaderIndex = currentLeaders.Count - 1;
        DisplayLeader();
    }

    // -------------------------
    // Card Interaction
    // -------------------------

    /// <summary>
    /// Adds or removes card from the player’s deck.
    /// </summary>
    private void HandleCardInteraction(CardUI cardUI)
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
        if (!deck.PlayerDeck.Contains(data) && !DeckManager.Instance.IsCardValidForPlayerDeck(data))
        {
            Debug.Log($"[DeckMenu] Cannot add [{data.name}] (faction mismatch)");
            return;
        }

        // Add or remove from deck
        if (deck.PlayerDeck.Contains(data))
        {
            deck.RemoveCard(data);
            cardUI.ShowCardSelectedOverlay(false);
            Debug.Log($"[DeckMenu] Removed [{data.name}] from deck");
        }
        else
        {
            deck.AddCard(data);
            cardUI.ShowCardSelectedOverlay(true);
            Debug.Log($"[DeckMenu] Added [{data.name}] to deck");
        }
    }

    // -------------------------
    // UI Helpers
    // -------------------------

    /// <summary>
    /// Shows or hides UI elements based on the current deck view.
    /// </summary>
    private void UpdateUIVisiblity()
    {
        bool isPlayerView = (viewingDeck == "Player");

        // Show/hide the leader card
        LeaderContainerPanel.gameObject.SetActive(isPlayerView);

        // Show/hide deck options
        DeckButtonsContainerPanel.gameObject.SetActive(isPlayerView);
    }

    /// <summary>
    /// Refreshes the deck display by adding/removing cards as needed.
    /// </summary>
    private void RefreshDeckView()
    {
        if (viewingDeck == "Player")
        {
            var deck = DeckManager.Instance.PlayerDeck;
            var cardsToRemove = new List<CardData>();

            // Remove cards no longer in deck
            foreach (var card in visibleCardsDict)
            {
                if (!deck.Contains(card.Key))
                    cardsToRemove.Add(card.Key);
            }
            foreach (var key in cardsToRemove)
            {
                CardManager.Instance.ReturnCard(visibleCardsDict[key]);
                visibleCardsDict.Remove(key);
            }

            // Update order and overlays
            for (int i = 0; i < deck.Count; i++)
            {
                var cardData = deck[i];
                if (visibleCardsDict.TryGetValue(cardData, out var cardUI))
                {
                    cardUI.ShowCardSelectedOverlay(false);
                    cardUI.transform.SetSiblingIndex(i);
                }
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
        foreach (var card in visibleCardsDict)
        {
            var cardUI = card.Value;
            cardUI.OnCardClicked -= HandleCardInteraction;
            CardManager.Instance.ReturnCard(cardUI);
        }
        visibleCardsDict.Clear();
    }

    /// <summary>
    /// Forces the layout system to update immediately so UI elements are positioned correctly.
    /// </summary>
    private void ForceLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(DeckCardsContentPanel as RectTransform);
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
