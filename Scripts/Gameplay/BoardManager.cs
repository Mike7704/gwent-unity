using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Controls gameplay setup and flow.
/// </summary>
public class BoardManager : Singleton<BoardManager>
{
    [Header("Rows")]
    public Transform PlayerHandRow, PlayerMeleeRow, PlayerRangedRow, PlayerSiegeRow;
    public Transform OpponentHandRow, OpponentMeleeRow, OpponentRangedRow, OpponentSiegeRow;

    [Header("Containers")]
    public Transform PlayerMeleeSpecialContainer, PlayerRangedSpecialContainer, PlayerSiegeSpecialContainer;
    public Transform OpponentMeleeSpecialContainer, OpponentRangedSpecialContainer, OpponentSiegeSpecialContainer;
    public Transform PlayerLeaderContainer, PlayerRecentCardContainer, PlayerSummonDeckContainer, PlayerDeckContainer, PlayerGraveyardContainer;
    public Transform OpponentLeaderContainer, OpponentRecentCardContainer, OpponentSummonDeckContainer, OpponentDeckContainer, OpponentGraveyardContainer;
    public Transform WeatherCardsContainer;

    [Header("Row Click Zones")]
    public Button PlayerMeleeSpecialZoneButton;
    public Button PlayerRangedSpecialZoneButton;
    public Button PlayerSiegeSpecialZoneButton;
    public Button PlayerMeleeRowZoneButton;
    public Button PlayerRangedRowZoneButton;
    public Button PlayerSiegeRowZoneButton;

    [Header("General UI")]
    public Button PassButton;
    public Button QuitButton;

    [Header("References")]
    public BoardUI boardUI;

    private BoardState state;
    private CardZoneManager zoneManager;
    public AbilityManager abilityManager;
    public AIOpponent aiOpponent;

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
    public bool playerHasActed = false;

    // Track last played card
    public CardData lastPlayedCard;
    public bool lastPlayedByPlayer;

    // Time/Delay values
    public readonly float turnDelay = 1.5f;
    public readonly float roundDelay = 2.5f;
    public readonly float aiThinkingTime = 1f;

    void Start()
    {
        Debug.Log("[BoardManager] Initialising...");

        cardUIMap.Clear(); // Make sure the mapping is clear
        state = new BoardState();
        zoneManager = new CardZoneManager(state, this, cardUIMap);
        abilityManager = new AbilityManager(state, this, zoneManager);
        zoneManager.SetAbilityManager(abilityManager);
        aiOpponent = new AIOpponent(state, this);

        InitialiseRowZoneButtons();
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

        boardUI.ShowBanner(Banner.PlayerTurn, $"Choose a card to redraw ({state.PlayerCardsRedrawn}/2) [PASS TO SKIP]");
        PassButton.GetComponentInChildren<TextMeshProUGUI>().text = "Skip";

        yield return new WaitUntil(() => state.PlayerCardsRedrawn >= 2);

        // Start the game
        boardUI.ShowBanner(Banner.PlayerTurn, $"Starting the game...");
        PassButton.GetComponentInChildren<TextMeshProUGUI>().text = "Pass";
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

        // Hide weather images
        boardUI.HideAllWeather();

        // Get card to keep on the board
        abilityManager.HandleMonstersAbilityStart();

        // Move all cards on both sides to graveyards
        zoneManager.MoveRowToGraveyard(state.playerMeleeSpecial, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.playerRangedSpecial, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.playerSiegeSpecial, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.playerMelee, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.playerRanged, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.playerSiege, isPlayer: true);
        zoneManager.MoveRowToGraveyard(state.opponentMeleeSpecial, isPlayer: false);
        zoneManager.MoveRowToGraveyard(state.opponentRangedSpecial, isPlayer: false);
        zoneManager.MoveRowToGraveyard(state.opponentSiegeSpecial, isPlayer: false);
        zoneManager.MoveRowToGraveyard(state.opponentMelee, isPlayer: false);
        zoneManager.MoveRowToGraveyard(state.opponentRanged, isPlayer: false);
        zoneManager.MoveRowToGraveyard(state.opponentSiege, isPlayer: false);
        zoneManager.MoveRowToGraveyard(state.weatherCards, isPlayer: false);

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
            // Do faction abilities at start of round
            yield return StartCoroutine(abilityManager.HandleMonstersAbilityEnd());
            yield return StartCoroutine(abilityManager.HandleNorthernRealmsAbility());
            yield return StartCoroutine(abilityManager.HandleScoiataelAbility());
            yield return StartCoroutine(abilityManager.HandleSkelligeAbility());
            UpdateBoardUI();

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
        state.CurrentRound++;

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
        else if (!abilityManager.HandleNilfgaardAbility())
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
        state.PlayerCardsRedrawn = 0;
        state.OpponentCardsRedrawn = 0;
        state.PlayerUsedFactionAbility = false;
        state.OpponentUsedFactionAbility = false;
        state.CurrentRound = 1;

        // Create a summon deck for both players
        CreateSummonDeck(CardDatabase.Instance.summonCards, PlayerSummonDeckContainer, isPlayer: true);
        CreateSummonDeck(CardDatabase.Instance.summonCards, OpponentSummonDeckContainer, isPlayer: false);

        // Create leader cards
        CreateLeader(DeckManager.Instance.PlayerLeader, PlayerLeaderContainer, isPlayer: true);
        CreateLeader(DeckManager.Instance.NPCLeader, OpponentLeaderContainer, isPlayer: false);

        // Create both decks from DeckManager
        CreateDeck(DeckManager.Instance.PlayerDeck, PlayerDeckContainer, isPlayer: true);
        CreateDeck(DeckManager.Instance.NPCDeck, OpponentDeckContainer, isPlayer: false);

        // Draw initial hands
        DrawInitialHand(isPlayer: true);
        DrawInitialHand(isPlayer: false);

        boardUI.HideAllWeather();
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
            if (originalCard.id < 0)
                newCard.id = originalCard.id; // These are special horn and mardroeme cards (keep negative IDs)
            else if (!isPlayer)
                newCard.id += 1000; // Offset opponent card IDs to avoid clashes
            copiedDeck.Add(newCard);
            CreateAndRegisterCard(newCard, summonContainer, visible: false);
        }

        if (isPlayer)
            state.playerSummonDeck = copiedDeck;
        else
            state.opponentSummonDeck = copiedDeck;
    }

    /// <summary>
    /// Generates a leader card from DeckManager and creates cardUI with mapping.
    /// </summary>
    /// <param name="sourceLeaderCard"></param>
    /// <param name="leaderContainer"></param>
    /// <param name="isPlayer"></param>
    private void CreateLeader(CardData sourceLeaderCard, Transform leaderContainer, bool isPlayer)
    {
        // Make a copy so the original DeckManager leader card isn't modified during gameplay
        List<CardData> copiedLeader = new List<CardData>();

        // Deep clone so each card is unique
        CardData newLeaderCard = sourceLeaderCard.Clone();
        if (!isPlayer)
            newLeaderCard.id += 1000; // Offset opponent card IDs to avoid clashes
        copiedLeader.Add(newLeaderCard);
        CreateAndRegisterCard(newLeaderCard, leaderContainer, visible: true);

        if (isPlayer)
            state.playerLeader = copiedLeader;
        else
            state.opponentLeader = copiedLeader;
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
            CreateAndRegisterCard(newCard, deckContainer, visible: false);
        }

        if (isPlayer)
            state.playerDeck = copiedDeck;
        else
            state.opponentDeck = copiedDeck;
    }

    /// <summary>
    /// Create UI for a given card and store the mapping
    /// </summary>
    public void CreateAndRegisterCard(CardData card, Transform deckContainer, bool visible)
    {
        CardUI cardUI = CardManager.Instance.CreateCard(card, cropped: true, deckContainer);
        cardUI.gameObject.SetActive(visible); // Hide the card until it's drawn
        cardUIMap[card] = cardUI;
        SetupCardInteraction(cardUI);
    }

    /// <summary>
    /// Draws a random initial hand from the deck.
    /// </summary>
    /// <param name="deck">Deck to draw from.</param>
    private void DrawInitialHand(bool isPlayer)
    {
        List<CardData> deck = isPlayer ? state.playerDeck : state.opponentDeck;

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
    /// Handles a card being selected for redraw at the start of the game.
    /// </summary>
    /// <param name="cardToRedraw"></param>
    /// <param name="isPlayer"></param>
    private void HandleRedrawSelection(CardData cardToRedraw, bool isPlayer)
    {
        List<CardData> deck = isPlayer ? state.playerDeck : state.opponentDeck;

        if (state.PlayerCardsRedrawn >= 2)
            return;

        // Draw a new random card
        int randomIndex = RandomUtils.GetRandom(0, deck.Count - 1);
        CardData drawnCard = deck[randomIndex];
        zoneManager.AddCardToHand(drawnCard, isPlayer);

        // Remove from hand back to deck
        zoneManager.AddCardToDeck(cardToRedraw, isPlayer);

        state.PlayerCardsRedrawn++;

        boardUI.ShowBanner(Banner.PlayerTurn, $"Choose a card to redraw ({state.PlayerCardsRedrawn}/2) [PASS TO SKIP]");
    }

    /// <summary>
    /// Handles a card being played to the board.
    /// </summary>
    public void HandleCardPlayed(CardData cardData, bool isPlayer)
    {
        if (isPlayer && (!state.IsPlayerTurn || !state.PlayerCanAct)) return;

        if (cardData == null)
        {
            Debug.LogError("[BoardManager] Null card played.");
            return;
        }

        // Handle Decoy ability separately
        if (cardData.ability == CardDefs.Ability.Decoy)
        {
            abilityManager.HandleDecoy(cardData, isPlayer);
            return;
        }
        // Handle horn and mardroeme special cards separately
        if (isPlayer && (cardData.type == CardDefs.Type.Special || cardData.type == CardDefs.Type.Leader) &&
            (cardData.ability == CardDefs.Ability.Horn || cardData.ability == CardDefs.Ability.Mardroeme))
        {
            abilityManager.HandleSpecialCard(cardData, isPlayer);
            return;
        }
        // Handle Agile cards separately
        if (isPlayer && cardData.range == CardDefs.Range.Agile)
        {
            abilityManager.HandleAgile(cardData, isPlayer);
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

        if (state.CurrentPhase == GamePhase.RedrawHand)
        {
            // Redraw phase, handle redraw selection
            HandleRedrawSelection(card, isPlayer: true);
        }
        else if (abilityManager.isDecoyActive)
        {
            // Decoy is active, handle decoy logic
            abilityManager.HandleDecoySwap(card, isPlayer: true);
        }
        else if (abilityManager.isSpecialCardActive)
        {
            // Special horn or mardroeme card is active, click the special card again to cancel
            abilityManager.CancelSpecialCardMode(card);
        }
        else if (abilityManager.isAgileActive)
        {
            // Agile is active, click the agile card again to cancel
            abilityManager.CancelAgileMode(card);
        }
        else if (abilityManager.isMedicActive)
        {
            // Medic is active, handle card to recover
            abilityManager.HandleMedicRecover(card, isPlayer: true);
        }
        else if (state.playerHand.Contains(card) || state.playerLeader.Contains(card))
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
    // Row Interaction Handlers
    // -------------------------

    /// <summary>
    /// Sets up row zone buttons listeners
    /// </summary>
    public void InitialiseRowZoneButtons()
    {
        PlayerMeleeSpecialZoneButton.onClick.AddListener(() => HandleRowClicked(RowZone.PlayerMeleeSpecial));
        PlayerRangedSpecialZoneButton.onClick.AddListener(() => HandleRowClicked(RowZone.PlayerRangedSpecial));
        PlayerSiegeSpecialZoneButton.onClick.AddListener(() => HandleRowClicked(RowZone.PlayerSiegeSpecial));

        PlayerMeleeRowZoneButton.onClick.AddListener(() => HandleRowClicked(RowZone.PlayerMeleeRow));
        PlayerRangedRowZoneButton.onClick.AddListener(() => HandleRowClicked(RowZone.PlayerRangedRow));
        PlayerSiegeRowZoneButton.onClick.AddListener(() => HandleRowClicked(RowZone.PlayerSiegeRow));

        DisableAllRowZoneButtons();
    }

    /// <summary>
    /// Handles clicks on row zones
    /// </summary>
    private void HandleRowClicked(RowZone row)
    {
        if (abilityManager.isAgileActive)
        {
            // Agile is active, handle row selection
            abilityManager.HandleAgileSelection(row, isPlayer: true);
        }
        else if (abilityManager.isSpecialCardActive)
        {
            // Special horn or mardroeme card is active, handle row selection
            abilityManager.HandleSpecialCardSelection(abilityManager.activeSpecialCard, row, isPlayer: true);
        }
    }

    /// <summary>
    /// Shows or hides row buttons for player to select
    /// </summary>
    public void EnableRowZoneButton(RowZone row, bool enable)
    {
        switch (row)
        {
            case RowZone.PlayerMeleeSpecial:
                PlayerMeleeSpecialZoneButton.gameObject.SetActive(enable);
                break;
            case RowZone.PlayerRangedSpecial:
                PlayerRangedSpecialZoneButton.gameObject.SetActive(enable);
                break;
            case RowZone.PlayerSiegeSpecial:
                PlayerSiegeSpecialZoneButton.gameObject.SetActive(enable);
                break;
            case RowZone.PlayerMeleeRow:
                PlayerMeleeRowZoneButton.gameObject.SetActive(enable);
                break;
            case RowZone.PlayerRangedRow:
                PlayerRangedRowZoneButton.gameObject.SetActive(enable);
                break;
            case RowZone.PlayerSiegeRow:
                PlayerSiegeRowZoneButton.gameObject.SetActive(enable);
                break;
            default:
                Debug.LogWarning("[BoardManager] No button for row: " + row);
                break;
        }

        boardUI.ShowRowHightlight(row, enable);
    }
    public void DisableAllRowZoneButtons()
    {
        PlayerMeleeSpecialZoneButton.gameObject.SetActive(false);
        PlayerRangedSpecialZoneButton.gameObject.SetActive(false);
        PlayerSiegeSpecialZoneButton.gameObject.SetActive(false);

        PlayerMeleeRowZoneButton.gameObject.SetActive(false);
        PlayerRangedRowZoneButton.gameObject.SetActive(false);
        PlayerSiegeRowZoneButton.gameObject.SetActive(false);

        boardUI.HideAllRowHighlights();
    }

    // -------------------------
    // General Input Handlers
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
        // Also used for skipping redraw at start of game
        if (isPlayer && state.CurrentPhase == GamePhase.RedrawHand)
        {
            state.PlayerCardsRedrawn = 2;
            return;
        }

        // Pass the round
        if (isPlayer && !state.PlayerHasPassed && state.CurrentPhase == GamePhase.PlayerTurn &&
            !abilityManager.isDecoyActive && !abilityManager.isMedicActive && !abilityManager.isAgileActive)
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
        state.playerMeleeSpecial.Clear();
        state.playerRangedSpecial.Clear();
        state.playerSiegeSpecial.Clear();
        state.playerMelee.Clear();
        state.playerRanged.Clear();
        state.playerSiege.Clear();
        state.opponentMeleeSpecial.Clear();
        state.opponentRangedSpecial.Clear();
        state.opponentSiegeSpecial.Clear();
        state.opponentMelee.Clear();
        state.opponentRanged.Clear();
        state.opponentSiege.Clear();
        state.playerGraveyard.Clear();
        state.opponentGraveyard.Clear();
        state.weatherCards.Clear();
    }
}
