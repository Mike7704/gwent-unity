using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeckMenu : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentPanel;  // Scroll View content
    public Button PlayerDeckButton;     // Button to show the player deck
    public Button BackButton;

    [Header("Faction Buttons")]
    public Button NorthernRealmsButton;
    public Button NilfgaardButton;
    public Button ScoiataelButton;
    public Button MonstersButton;
    public Button SkelligeButton;

    private List<CardUI> visibleCards = new List<CardUI>();
    private bool isShowingPlayerDeck = false;

    public static DeckMenu Instance; // For CardUI click reference

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    void Start()
    {
        // Setup faction buttons
        NorthernRealmsButton.onClick.AddListener(() => DisplayFaction("Northern Realms"));
        NilfgaardButton.onClick.AddListener(() => DisplayFaction("Nilfgaard"));
        ScoiataelButton.onClick.AddListener(() => DisplayFaction("Scoiatael"));
        MonstersButton.onClick.AddListener(() => DisplayFaction("Monsters"));
        SkelligeButton.onClick.AddListener(() => DisplayFaction("Skellige"));

        // Setup My Deck button
        PlayerDeckButton.onClick.AddListener(() => DisplayPlayerDeck());

        DisplayPlayerDeck(); // Default view
    }

    /// <summary>
    /// Show all cards for a given faction.
    /// Uses the CardPool instead of instantiating new cards.
    /// </summary>
    public void DisplayFaction(string faction)
    {
        isShowingPlayerDeck = false;
        ClearScrollPane();

        var cards = CardDatabase.Instance.GetCardsByFaction(faction);

        foreach (var data in cards)
        {
            CardUI cardUI = CardPool.Instance.GetCard(contentPanel);
            if (cardUI != null)
            {
                cardUI.Setup(data);
                visibleCards.Add(cardUI);
            }
            else
            {
                Debug.LogWarning("CardPool exhausted! Not enough preloaded cards for faction display.");
            }
        }
    }

    /// <summary>
    /// Show the player deck in the scroll view.
    /// </summary>
    public void DisplayPlayerDeck()
    {
        isShowingPlayerDeck = true;
        ClearScrollPane();

        var playerDeck = DeckManager.Instance.playerDeck;
        foreach (var data in playerDeck)
        {
            CardUI cardUI = CardPool.Instance.GetCard(contentPanel);
            if (cardUI != null)
            {
                cardUI.Setup(data);
                visibleCards.Add(cardUI);
            }
        }
    }

    /// <summary>
    /// Called by CardUI when a card is clicked.
    /// Adds/removes card from the player deck and enforces faction restriction.
    /// </summary>
    public void OnCardClicked(CardUI cardUI)
    {
        if (cardUI == null || cardUI.cardData == null) return;

        var cardData = cardUI.cardData;
        var deckManager = DeckManager.Instance;

        // Pick the first faction if not yet selected
        if (string.IsNullOrEmpty(deckManager.selectedFaction) && cardData.faction != "Neutral" && cardData.type != "special")
        {
            deckManager.selectedFaction = cardData.faction;
            Debug.Log($"Faction selected: {deckManager.selectedFaction}");
        }

        // Restrict to selected faction (except neutral/special)
        if (cardData.faction != deckManager.selectedFaction && cardData.type != "special" && cardData.faction != "Neutral")
        {
            Debug.Log("Cannot add card: faction mismatch");
            return;
        }

        // Add or remove card from deck
        if (deckManager.ContainsCard(cardData))
        {
            deckManager.RemoveCard(cardData);
            Debug.Log($"Removed {cardData.name} from player deck");
        }
        else
        {
            deckManager.AddCard(cardData);
            Debug.Log($"Added {cardData.name} to player deck");
        }

        // Refresh player view if active
        if (isShowingPlayerDeck)
            DisplayPlayerDeck();
    }

    /// <summary>
    /// Return all visible cards to the pool and clear the scroll view.
    /// </summary>
    private void ClearScrollPane()
    {
        foreach (var card in visibleCards)
            CardPool.Instance.ReturnCard(card);
        visibleCards.Clear();
    }

    /// <summary>
    /// Called when Back button is clicked
    /// </summary>
    public void BackToMainMenu()
    {
        DeckManager.Instance.SaveDeck();
        ClearScrollPane();
        StartCoroutine(LoadMainMenuAsync());
    }

    private IEnumerator LoadMainMenuAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");
        asyncLoad.allowSceneActivation = true;
        yield return asyncLoad;
    }
}
