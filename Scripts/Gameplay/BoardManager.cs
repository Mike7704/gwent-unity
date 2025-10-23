using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls gameplay setup and flow.
/// </summary>
public class BoardManager : Singleton<BoardManager>
{
    [Header("Rows")]
    public Transform PlayerHandRow, PlayerMeleeRow, PlayerRangedRow, PlayerSiegeRow;
    public Transform OpponentHandRow, OpponentMeleeRow, OpponentRangedRow, OpponentSiegeRow;

    [Header("Containers")]
    public Transform PlayerLeaderContainer, PlayerRecentCardContainer, PlayerDeckContainer, PlayerGraveyardContainer;
    public Transform OpponentLeaderContainer, OpponentRecentCardContainer, OpponentDeckContainer, OpponentGraveyardContainer;

    [Header("References")]
    public BoardUI boardUI;

    private BoardState state;
    private CardZoneManager zoneManager;
    private AIOpponent aiOpponent;

    // Mapping of CardData to CardUI for easy access
    private readonly Dictionary<CardData, CardUI> cardUIMap = new();

    // Game settings
    private int initialHandSize = 10;
    private bool randomisePlayerDeck = false;
    private int randomiseDeckSize = 25;
    private int spyDrawAmount = 2;
    private bool leaderCardsEnabled = true;
    private bool factionAbilityEnabled = true;

    // Check if player has acted their turn
    private bool playerHasActed = false;

    // Time/Delay values
    private float turnDelay = 1f;
    private float aiThinkingTime = 1f;

    void Start()
    {
        Debug.Log("[BoardManager] Initialising...");

        cardUIMap.Clear(); // Make sure the mapping is clear
        state = new BoardState();
        zoneManager = new CardZoneManager(state, this, cardUIMap);
        aiOpponent = new AIOpponent(state, this);

        // Start the game loop
        StartCoroutine(GameLoop());
    }

    // -------------------------
    // Game Loop Phase Management
    // -------------------------

    /// <summary>
    /// Game loop to manage phases and turns.
    /// </summary>
    private IEnumerator GameLoop()
    {
        SetGamePhase(GamePhase.Start);

        while (!state.IsGameOver)
        {
            switch (state.CurrentPhase)
            {
                case GamePhase.Start:
                    yield return HandleGameSetup(); // Initial game setup
                    SetGamePhase(GamePhase.RedrawHand);
                    break;

                case GamePhase.RedrawHand:
                    yield return HandleRedrawHand(); // Redraw from hand at start of game
                    SetGamePhase(GamePhase.RoundStart);
                    break;

                case GamePhase.PlayerTurn:
                    state.IsPlayerTurn = true;
                    state.PlayerCanAct = true;
                    playerHasActed = false;
                    UpdateBoardUI();
                    yield return WaitForPlayerAction(); // Wait for card play or pass
                    state.PlayerCanAct = false;
                    SetGamePhase(GamePhase.OpponentTurn);
                    break;

                case GamePhase.OpponentTurn:
                    state.IsPlayerTurn = false;
                    state.PlayerCanAct = false;
                    UpdateBoardUI();
                    yield return WaitForOpponentAction(); // Wait for card play or pass
                    SetGamePhase(GamePhase.PlayerTurn);
                    break;

                case GamePhase.ResolvingCard:
                    yield return WaitForCardResolve(); // Wait for card ability
                    DetermineNextGamePhase();
                    break;

                case GamePhase.RoundStart:
                    state.IsRoundOver = false;
                    UpdateBoardUI();
                    yield return HandleRoundStart(); // Handle start of round
                    break;

                case GamePhase.RoundEnd:
                    state.IsRoundOver = true;
                    UpdateBoardUI();
                    yield return HandleRoundEnd(); // Handle end of round
                    break;

                case GamePhase.GameOver:
                    HandleGameEnd(); // Handle game over
                    yield break;
            }

            yield return null; // Wait one frame
        }
    }

    // -------------------------
    // Game Phase Handlers
    // -------------------------

    public void SetGamePhase(GamePhase phase)
    {
        state.CurrentPhase = phase;
    }

    private void DetermineNextGamePhase()
    {
        SetGamePhase(state.IsPlayerTurn ? GamePhase.PlayerTurn : GamePhase.OpponentTurn);
    }

    private IEnumerator HandleGameSetup()
    {
        Debug.Log($"[BoardManager] Setting up the game...");

        SetupGameSettings();
        SetupBoardUI();
        StartGame();
        yield break; 
    }

    private IEnumerator HandleRedrawHand()
    {
        Debug.Log($"[BoardManager] Waiting for hand redraw...");

        // Redraw logic
        yield break;
    }

    private IEnumerator WaitForPlayerAction()
    {
        Debug.Log($"[BoardManager] Player's turn...");

        while (!playerHasActed)
            yield return null; // Wait one frame

        yield return new WaitForSeconds(turnDelay);
    }

    private IEnumerator WaitForOpponentAction()
    {
        Debug.Log($"[BoardManager] Opponent's turn...");

        yield return new WaitForSeconds(aiThinkingTime);

        yield return StartCoroutine(aiOpponent.PlayTurn());

        yield return new WaitForSeconds(turnDelay);
    }

    private IEnumerator WaitForCardResolve()
    {
        Debug.Log($"[BoardManager] Card resolving...");

        // Card ability logic
        yield break;
    }

    private IEnumerator HandleRoundStart()
    {
        Debug.Log($"[BoardManager] Round started.");

        // Decide who starts first
        state.IsPlayerTurn = RandomUtils.GetRandom(0, 1) == 1;
        SetGamePhase(state.IsPlayerTurn ? GamePhase.PlayerTurn : GamePhase.OpponentTurn);
        yield break;
    }

    private IEnumerator HandleRoundEnd()
    {
        Debug.Log($"[BoardManager] Round ended.");

        if (state.PlayerLife == 0 || state.OpponentLife == 0)
            SetGamePhase(GamePhase.GameOver);
        else
            SetGamePhase(GamePhase.RoundStart);
        yield break;
    }

    private void HandleGameEnd()
    {
        Debug.Log($"[BoardManager] Game ended.");

        state.IsGameOver = true;
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }

    // -------------------------
    // Game Setup Methods
    // -------------------------

    /// <summary>
    /// Sets up game settings.
    /// </summary>
    private void SetupGameSettings()
    {
        initialHandSize = SettingsManager.Instance.InitialHandSize;
        randomisePlayerDeck = SettingsManager.Instance.RandomisePlayerDeck;
        randomiseDeckSize = SettingsManager.Instance.RandomiseDeckSize;
        spyDrawAmount = SettingsManager.Instance.SpyDrawAmount;
        leaderCardsEnabled = SettingsManager.Instance.LeaderCardEnabled;
        factionAbilityEnabled = SettingsManager.Instance.FactionAbilityEnabled;

        if (randomisePlayerDeck)
            DeckManager.Instance.RandomiseDeck(DeckManager.Instance.PlayerDeck, randomiseDeckSize);

        // Generate a random deck for the NPC
        DeckManager.Instance.RandomiseDeck(DeckManager.Instance.NPCDeck, randomiseDeckSize);
    }

    /// <summary>
    /// Sets up the board UI with player and opponent info.
    /// </summary>
    private void SetupBoardUI()
    {
        boardUI.SetupPlayersInfo(
            ProfileManager.Instance.Username,
            DeckManager.Instance.PlayerFaction,
            "Opponent",
            DeckManager.Instance.NPCFaction);
    }

    /// <summary>
    /// Starts a new game by setting up decks, drawing hands, and initialising the turn.
    /// </summary>
    private void StartGame()
    {
        // Set starting lives
        state.PlayerLife = 2;
        state.OpponentLife = 2;

        // Create both decks from DeckManager
        CreateDeck(DeckManager.Instance.PlayerDeck, PlayerDeckContainer, isPlayer: true);
        CreateDeck(DeckManager.Instance.NPCDeck, OpponentDeckContainer, isPlayer: false);

        // Draw initial hands
        DrawInitialHand(state.playerDeck, isPlayer: true);
        DrawInitialHand(state.opponentDeck, isPlayer: false);

        UpdateBoardUI();
    }

    /// <summary>
    /// Generates a deck from DeckManager and creates cardUI with mapping.
    /// </summary>
    /// <param name="sourceDeck"></param>
    /// <param name="handRow"></param>
    /// <param name="isPlayer"></param>
    private void CreateDeck(List<CardData> sourceDeck, Transform deckContainer, bool isPlayer)
    {
        // Make a copy so the original DeckManager list isn't modified during gameplay
        List<CardData> copiedDeck = new List<CardData>();

        foreach (var originalCard in sourceDeck)
        {
            // Deep clone so each card is unique
            CardData newCard = originalCard.Clone();
            if (!isPlayer)
                newCard.id += 1000; // Offset opponent card IDs to avoid clashes
            copiedDeck.Add(newCard);

            // Create UI for each card and store the mapping
            CardUI cardUI = CardManager.Instance.CreateCard(newCard, cropped: true, deckContainer);
            cardUI.gameObject.SetActive(false); // Hide the card until it's drawn
            cardUIMap[newCard] = cardUI;
            SetupCardInteraction(cardUI);
        }

        if (isPlayer)
            state.playerDeck = copiedDeck;
        else
            state.opponentDeck = copiedDeck;
    }

    /// <summary>
    /// Draws a random initial hand from the deck.
    /// </summary>
    /// <param name="deck">Deck to draw from.</param>
    /// <param name="hand">Hand to add drawn cards to.</param>
    /// <param name="handRow">UI row where cards should appear.</param>
    /// <param name="handSize">Number of cards to draw.</param>
    private void DrawInitialHand(List<CardData> deck, bool isPlayer)
    {
        for (int i = 0; i < initialHandSize && deck.Count > 0; i++)
        {
            int randomIndex = RandomUtils.GetRandom(0, deck.Count - 1);
            CardData drawnCard = deck[randomIndex];

            zoneManager.AddCardToHand(drawnCard, isPlayer);
        }
    }

    // -------------------------
    // Card Interaction Handlers
    // -------------------------

    /// <summary>
    /// Handles a card being played to the board.
    /// </summary>
    /// <param name="cardUI"></param>
    public void HandleCardPlayed(CardData cardData, bool isPlayer)
    {
        if (isPlayer && (!state.IsPlayerTurn || !state.PlayerCanAct)) return;

        if (cardData == null)
        {
            Debug.LogError("[BoardManager] Null card played.");
            return;
        }

        state.PlayerCanAct = false;

        zoneManager.AddCardToBoard(cardData, isPlayer);

        if (isPlayer)
            playerHasActed = true;
    }

    /// <summary>
    /// Sets up event listeners for the card.
    /// </summary>
    /// <param name="card"></param>
    private void SetupCardInteraction(CardUI card)
    {
        card.OnCardClicked += HandleCardClicked;
    }

    /// <summary>
    /// Handles clicks on cards based on their zone.
    /// </summary>
    /// <param name="cardUI"></param>
    private void HandleCardClicked(CardUI cardUI)
    {
        if (cardUI == null || cardUI.cardData == null)
        {
            Debug.LogError("[BoardManager] Null card clicked.");
            return;
        }

        CardData card = cardUI.cardData;

        if (state.playerHand.Contains(card))
        {
            // Card is in player's hand
            HandleCardPlayed(cardUI.cardData, isPlayer: true);
        }
        else if (state.playerGraveyard.Contains(card))
        {
            // Card is in graveyard
        }
        else 
        {
            // Card is on the board
        }
    }

    // -------------------------
    // Update Board UI
    // -------------------------

    /// <summary>
    /// Updates the board UI to reflect the current state.
    /// </summary>
    public void UpdateBoardUI()
    {
        boardUI.UpdateUI(state);
    }
}
