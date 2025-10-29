using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Controls gameplay setup and flow.
/// </summary>
public class BoardManager : Singleton<BoardManager>
{
    [Header("Rows")]
    public Transform PlayerHandRow, PlayerMeleeRow, PlayerRangedRow, PlayerSiegeRow;
    public Transform OpponentHandRow, OpponentMeleeRow, OpponentRangedRow, OpponentSiegeRow;

    [Header("Containers")]
    public Transform PlayerLeaderContainer, PlayerRecentCardContainer, PlayerSummonDeckContainer, PlayerDeckContainer, PlayerGraveyardContainer;
    public Transform OpponentLeaderContainer, OpponentRecentCardContainer, OpponentSummonDeckContainer, OpponentDeckContainer, OpponentGraveyardContainer;

    [Header("General UI")]
    public Button PassButton;
    public Button QuitButton;

    [Header("References")]
    public BoardUI boardUI;

    private BoardState state;
    private CardZoneManager zoneManager;
    private AbilityManager abilityManager;
    private AIOpponent aiOpponent;

    // Mapping of CardData to CardUI for easy access
    private readonly Dictionary<CardData, CardUI> cardUIMap = new();

    // Game settings
    private int initialHandSize = 10;
    private bool randomisePlayerDeck = false;
    private int randomiseDeckSize = 25;
    public int spyDrawAmount = 2;
    public bool leaderCardsEnabled = true;
    public bool factionAbilityEnabled = true;

    // Player and Opponent info
    public string playerUsername;
    public string opponentUsername;
    public string playerFaction;
    public string opponentFaction;

    // Game variables
    private bool playerHasActed = false;

    // Track last played card
    private CardData lastPlayedCard;
    private bool lastPlayedByPlayer;

    // Time/Delay values
    private readonly float turnDelay = 1.5f;
    private readonly float roundDelay = 2.5f;
    private readonly float aiThinkingTime = 1f;

    void Start()
    {
        Debug.Log("[BoardManager] Initialising...");

        cardUIMap.Clear(); // Make sure the mapping is clear
        state = new BoardState();
        zoneManager = new CardZoneManager(state, this, cardUIMap);
        abilityManager = new AbilityManager(state, this, zoneManager);
        zoneManager.SetAbilityManager(abilityManager);
        aiOpponent = new AIOpponent(state, this);

        InitialiseGeneralButtons();

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
                    HandleGameSetup(); // Initial game setup
                    break;

                case GamePhase.RedrawHand:
                    yield return HandleRedrawHand(); // Redraw from hand at start of game
                    break;

                case GamePhase.PlayerTurn:
                    yield return WaitForPlayerAction(); // Wait for card play or pass
                    break;

                case GamePhase.OpponentTurn:
                    yield return WaitForOpponentAction(); // Wait for card play or pass
                    break;

                case GamePhase.ResolvingCard:
                    yield return WaitForCardResolve(); // Wait for card ability
                    break;

                case GamePhase.RoundStart:
                    yield return HandleRoundStart(); // Handle start of round
                    break;

                case GamePhase.RoundEnd:
                    yield return HandleRoundEnd(); // Handle end of round
                    break;

                case GamePhase.GameOver:
                    HandleGameEnd(); // Handle game over
                    break;
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

    private IEnumerator TransitionToNextGamePhase()
    {
        yield return new WaitForSeconds(turnDelay);

        if (state.PlayerHasPassed && state.OpponentHasPassed)
        {
            SetGamePhase(GamePhase.RoundEnd);
        }
        else if (state.PlayerHasPassed)
        {
            SetGamePhase(GamePhase.OpponentTurn);
        }
        else if (state.OpponentHasPassed)
        {
            SetGamePhase(GamePhase.PlayerTurn);
        }
        else
        {
            SetGamePhase(state.IsPlayerTurn ? GamePhase.OpponentTurn : GamePhase.PlayerTurn);
        }
    }

    private void HandleGameSetup()
    {
        Debug.Log($"[BoardManager] Setting up the board...");

        AudioSystem.Instance.PlaySFX(SFX.StartGame);

        SetupGameSettings();
        SetupBoardUI();
        StartGame();

        SetGamePhase(GamePhase.RedrawHand);
    }

    private IEnumerator HandleRedrawHand()
    {
        Debug.Log($"[BoardManager] Waiting for hand redraw...");

        AudioSystem.Instance.PlaySFX(SFX.RedrawCardsStart);

        boardUI.ShowBanner(Banner.PlayerTurn, $"Choose a card to redraw: {"cardsRedrawn"}/2 [SKIP]");
        yield return new WaitForSeconds(1f);

        AudioSystem.Instance.PlaySFX(SFX.RedrawCardsEnd);

        SetGamePhase(GamePhase.RoundStart);
    }

    private IEnumerator WaitForPlayerAction()
    {
        if (state.PlayerHasPassed) yield break;

        Debug.Log($"[BoardManager] Player's turn...");

        if (!state.OpponentHasPassed)
            AudioSystem.Instance.PlaySFX(SFX.TurnPlayer);

        state.IsPlayerTurn = true;
        state.PlayerCanAct = true;
        playerHasActed = false;
        boardUI.HideBanner();
        UpdateBoardUI();

        while (!playerHasActed)
            yield return null; // Wait one frame

        state.PlayerCanAct = false;
    }

    private IEnumerator WaitForOpponentAction()
    {
        if (state.OpponentHasPassed) yield break;

        Debug.Log($"[BoardManager] Opponent's turn...");

        if (!state.PlayerHasPassed)
            AudioSystem.Instance.PlaySFX(SFX.TurnOpponent);

        state.IsPlayerTurn = false;
        state.PlayerCanAct = false;
        boardUI.HideBanner();
        UpdateBoardUI();

        yield return new WaitForSeconds(aiThinkingTime);

        yield return StartCoroutine(aiOpponent.PlayTurn());
    }

    private IEnumerator WaitForCardResolve()
    {
        abilityManager.ResetResolvedAbilities();

        if (lastPlayedCard != null)
        {
            // Resolve the last played card's ability
            yield return StartCoroutine(abilityManager.ResolveCard(lastPlayedCard, lastPlayedByPlayer));
        }

        UpdateBoardUI();

        lastPlayedCard = null; // Clear the last played card
        abilityManager.ResetResolvedAbilities();

        yield return StartCoroutine(TransitionToNextGamePhase());
    }

    private IEnumerator HandleRoundStart()
    {
        Debug.Log($"[BoardManager] Starting round...");

        state.PlayerHasPassed = false;
        state.OpponentHasPassed = false;

        // Move all cards on both sides to graveyards
        zoneManager.MoveRowToGraveyard(state.playerMelee, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.playerRanged, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.playerSiege, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.opponentMelee, isPlayer: false);
        zoneManager.MoveRowToGraveyard(state.opponentRanged, isPlayer: false);
        zoneManager.MoveRowToGraveyard(state.opponentSiege, isPlayer: false);

        // Add any cards from avenger cards to the board
        yield return StartCoroutine(abilityManager.ResolveQueuedAvengers());

        UpdateBoardUI();

        // Decide who starts
        if (state.PlayerLife == 2 && state.OpponentLife == 2)
        {
            // Coin toss
            AudioSystem.Instance.PlaySFX(SFX.CoinFlip);
            yield return new WaitForSeconds(roundDelay);

            state.IsPlayerTurn = RandomUtils.GetRandom(0, 1) == 1;
            if (state.IsPlayerTurn)
                boardUI.ShowBanner(Banner.CoinPlayer, "You will go first");
            else
                boardUI.ShowBanner(Banner.CoinOpponent, "Your opponent will go first");
        }
        else
        {
            boardUI.ShowBanner(Banner.PlayerTurn, "Starting the next round...");
            yield return new WaitForSeconds(roundDelay);

            state.IsPlayerTurn = !state.IsPlayerTurn;
            if (state.IsPlayerTurn)
                boardUI.ShowBanner(Banner.PlayerTurn, "Your turn");
            else
                boardUI.ShowBanner(Banner.OpponentTurn, "Opponent's turn");
        }

        yield return new WaitForSeconds(roundDelay);

        SetGamePhase(state.IsPlayerTurn ? GamePhase.PlayerTurn : GamePhase.OpponentTurn);
    }

    private IEnumerator HandleRoundEnd()
    {
        Debug.Log($"[BoardManager] Round ended.");

        state.IsRoundOver = true;

        int playerScore = state.GetPlayerTotalScore();
        int opponentScore = state.GetOpponentTotalScore();
        state.RecordRoundScores();

        if (playerScore > opponentScore)
        {
            Debug.Log("[BoardManager] Player wins the round!");
            AudioSystem.Instance.PlaySFX(SFX.RoundWin);
            state.OpponentLife--;
            boardUI.ShowBanner(Banner.RoundWin, "You won the round");
        }
        else if (playerScore < opponentScore)
        {
            Debug.Log("[BoardManager] Opponent wins the round!");
            AudioSystem.Instance.PlaySFX(SFX.RoundLoss);
            state.PlayerLife--;
            boardUI.ShowBanner(Banner.RoundLoss, "You lost the round");
        }
        else
        {
            Debug.Log("[BoardManager] It's a draw!");
            AudioSystem.Instance.PlaySFX(SFX.RoundDraw);
            state.PlayerLife--;
            state.OpponentLife--;
            boardUI.ShowBanner(Banner.RoundDraw, "You drew the round");
        }

        UpdateBoardUI();

        yield return new WaitForSeconds(roundDelay);

        if (state.PlayerLife == 0 || state.OpponentLife == 0)
            SetGamePhase(GamePhase.GameOver);
        else
            SetGamePhase(GamePhase.RoundStart);
    }

    private void HandleGameEnd()
    {
        Debug.Log($"[BoardManager] Game ended.");

        state.IsGameOver = true;

        if (state.PlayerLife == 0 && state.OpponentLife == 0)
        {
            Debug.Log("[BoardManager] The game ends in a draw!");
            AudioSystem.Instance.PlaySFX(SFX.RoundDraw);
            boardUI.ShowEndScreen(EndScreen.Draw, this, state);
        }
        else if (state.OpponentLife == 0)
        {
            Debug.Log("[BoardManager] Player wins the game!");
            AudioSystem.Instance.PlaySFX(SFX.GameWin);
            boardUI.ShowEndScreen(EndScreen.Win, this, state);
        }
        else
        {
            Debug.Log("[BoardManager] Opponent wins the game!");
            AudioSystem.Instance.PlaySFX(SFX.GameLoss);
            boardUI.ShowEndScreen(EndScreen.Lose, this, state);
        }
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
        playerUsername = ProfileManager.Instance.Username;
        opponentUsername = "Opponent";
        playerFaction = DeckManager.Instance.PlayerFaction;
        opponentFaction = DeckManager.Instance.NPCFaction;

        boardUI.SetupPlayersInfo(playerUsername, playerFaction, opponentUsername, opponentFaction);
    }

    /// <summary>
    /// Starts a new game by setting up decks, drawing hands, and initialising the turn.
    /// </summary>
    private void StartGame()
    {
        // Set default states
        state.PlayerCanAct = false;
        state.IsCardResolving = false;
        state.IsPlayerTurn = false;
        state.PlayerHasPassed = false;
        state.OpponentHasPassed = false;
        state.IsRoundOver = false;
        state.IsGameOver = false;
        state.PlayerLife = 2;
        state.OpponentLife = 2;

        // Create a summon deck for both players
        CreateSummonDeck(CardDatabase.Instance.summonCards, PlayerSummonDeckContainer, isPlayer: true);
        CreateSummonDeck(CardDatabase.Instance.summonCards, OpponentSummonDeckContainer, isPlayer: false);

        // Create both decks from DeckManager
        CreateDeck(DeckManager.Instance.PlayerDeck, PlayerDeckContainer, isPlayer: true);
        CreateDeck(DeckManager.Instance.NPCDeck, OpponentDeckContainer, isPlayer: false);

        // Draw initial hands
        DrawInitialHand(state.playerDeck, isPlayer: true);
        DrawInitialHand(state.opponentDeck, isPlayer: false);

        boardUI.HideBanner();
        boardUI.HideEndScreen();
        UpdateBoardUI();
    }

    /// <summary>
    /// Generates a deck of cards that can be summoned and creates cardUI with mapping.
    /// </summary>
    /// <param name="sourceDeck"></param>
    /// <param name="summonContainer"></param>
    /// <param name="isPlayer"></param>
    private void CreateSummonDeck(List<CardData> sourceDeck, Transform summonContainer, bool isPlayer)
    {
        List<CardData> copiedDeck = new List<CardData>();

        foreach (var originalCard in sourceDeck)
        {
            // Only include neutral cards and cards matching the faction
            if (originalCard.faction != CardDefs.Faction.Neutral &&
                ((isPlayer && originalCard.faction != playerFaction) ||
                 (!isPlayer && originalCard.faction != opponentFaction)))
            {
                continue;
            }

            // Deep clone so each card is unique
            CardData newCard = originalCard.Clone();
            if (!isPlayer)
                newCard.id += 1000; // Offset opponent card IDs to avoid clashes
            copiedDeck.Add(newCard);

            // Create hidden UI
            CardUI cardUI = CardManager.Instance.CreateCard(newCard, cropped: true, summonContainer);
            cardUI.gameObject.SetActive(false); // Hide the card until it's drawn
            cardUIMap[newCard] = cardUI;
            SetupCardInteraction(cardUI);
        }

        if (isPlayer)
            state.playerSummonDeck = copiedDeck;
        else
            state.opponentSummonDeck = copiedDeck;
    }

    /// <summary>
    /// Generates a deck from DeckManager and creates cardUI with mapping.
    /// </summary>
    /// <param name="sourceDeck"></param>
    /// <param name="deckContainer"></param>
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

        // Store which card was played
        lastPlayedCard = cardData;
        lastPlayedByPlayer = isPlayer;

        // Do card ability
        SetGamePhase(GamePhase.ResolvingCard);

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
    // Input Handlers
    // -------------------------

    /// <summary>
    /// Check for key inputs
    /// </summary>
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PassRound(isPlayer: true);
        }
        else if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ForfeitGame();
        }
    }

    /// <summary>
    /// Sets up general buttons listeners
    /// </summary>
    public void InitialiseGeneralButtons()
    {
        PassButton.onClick.AddListener(() => PassRound(isPlayer: true));
        QuitButton.onClick.AddListener(ForfeitGame);
    }

    // -------------------------
    // Pass Handler
    // -------------------------

    /// <summary>
    /// End turn and pass the current round
    /// </summary>
    /// <param name="isPlayer"></param>
    public void PassRound(bool isPlayer)
    {
        if (isPlayer && !state.PlayerHasPassed && state.CurrentPhase == GamePhase.PlayerTurn)
        {
            Debug.Log("[BoardManager] Player has passed.");
            state.PlayerHasPassed = true;
            playerHasActed = true;
            state.PlayerCanAct = false;
            AudioSystem.Instance.PlaySFX(SFX.TurnOpponent);
            boardUI.ShowBanner(Banner.RoundPassed, "You passed");
            UpdateBoardUI();
            StartCoroutine(TransitionToNextGamePhase());
        }
        else if (!isPlayer && !state.OpponentHasPassed && state.CurrentPhase == GamePhase.OpponentTurn)
        {
            Debug.Log("[BoardManager] Opponent has passed.");
            state.OpponentHasPassed = true;
            AudioSystem.Instance.PlaySFX(SFX.TurnPlayer);
            boardUI.ShowBanner(Banner.RoundPassed, "Your opponent has passed");
            UpdateBoardUI();
            StartCoroutine(TransitionToNextGamePhase());
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
        abilityManager.CalculateAllCardStrengths();
        boardUI.UpdateUI(state);
    }

    // -------------------------
    // Quit Game
    // -------------------------

    /// <summary>
    /// Player clicks the exit button
    /// </summary>
    private void ForfeitGame()
    {
        if (ConfirmationWindow.Instance.isVisible) return;

        ConfirmationWindow.Instance.Show(
            "Forfeit",
            "Do you want to forfeit?",
            () => {
                Debug.Log("[BoardManager] Player has forfeited.");
                QuitGame();
            }
        );
    }

    /// <summary>
    /// End the game and display the main menu
    /// </summary>
    public void QuitGame()
    {
        StopAllCoroutines();
        CleanupBoard();
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }

    /// <summary>
    /// Cleans the board before exiting
    /// </summary>
    public void CleanupBoard()
    {
        // Unsubscribe all card click events
        foreach (var kvp in cardUIMap)
        {
            var cardUI = kvp.Value;
            if (cardUI != null)
                cardUI.OnCardClicked -= HandleCardClicked;
        }

        // Destroy all card GameObjects
        foreach (var kvp in cardUIMap)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }

        cardUIMap.Clear();

        // Clear state zones
        state.playerSummonDeck.Clear();
        state.opponentSummonDeck.Clear();
        state.playerDeck.Clear();
        state.opponentDeck.Clear();
        state.playerHand.Clear();
        state.opponentHand.Clear();
        state.playerMelee.Clear();
        state.playerRanged.Clear();
        state.playerSiege.Clear();
        state.opponentMelee.Clear();
        state.opponentRanged.Clear();
        state.opponentSiege.Clear();
        state.playerGraveyard.Clear();
        state.opponentGraveyard.Clear();
    }
}
