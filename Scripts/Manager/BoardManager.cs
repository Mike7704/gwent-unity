using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Handles the board state, player/opponent hands, rows, and turn flow during gameplay.
/// </summary>
public class BoardManager : Singleton<BoardManager>
{
    // -------------------------
    // Board Structure
    // -------------------------

    [Header("Player Rows")]
    public Transform PlayerHandRow;
    public Transform PlayerMeleeRow;
    public Transform PlayerRangedRow;
    public Transform PlayerSiegeRow;

    [Header("Opponent Rows")]
    public Transform OpponentHandRow;
    public Transform OpponentMeleeRow;
    public Transform OpponentRangedRow;
    public Transform OpponentSiegeRow;

    [Header("Player Containers")]
    public Transform PlayerLeaderContainer;
    public Transform PlayerRecentCardContainer;
    public Transform PlayerDeckContainer;
    public Transform PlayerGraveyardContainer;

    [Header("Opponent Containers")]
    public Transform OpponentLeaderContainer;
    public Transform OpponentRecentCardContainer;
    public Transform OpponentDeckContainer;
    public Transform OpponentGraveyardContainer;

    // -------------------------
    // UI References
    // -------------------------

    [Header("Player UI")]
    public TextMeshProUGUI PlayerName;
    public TextMeshProUGUI PlayerFaction;
    public TextMeshProUGUI PlayerHandSize;
    public TextMeshProUGUI PlayerDeckSize;
    public TextMeshProUGUI PlayerTotalScore;
    public TextMeshProUGUI PlayerMeleeScore;
    public TextMeshProUGUI PlayerRangedScore;
    public TextMeshProUGUI PlayerSiegeScore;

    [Header("Opponent UI")]
    public TextMeshProUGUI OpponentName;
    public TextMeshProUGUI OpponentFaction;
    public TextMeshProUGUI OpponentHandSize;
    public TextMeshProUGUI OpponentDeckSize;
    public TextMeshProUGUI OpponentTotalScore;
    public TextMeshProUGUI OpponentMeleeScore;
    public TextMeshProUGUI OpponentRangedScore;
    public TextMeshProUGUI OpponentSiegeScore;

    // -------------------------
    // Gameplay State
    // -------------------------

    private List<CardData> playerHand = new();
    private List<CardData> opponentHand = new();

    private List<CardData> playerMelee = new();
    private List<CardData> playerRanged = new();
    private List<CardData> playerSiege = new();

    private List<CardData> opponentMelee = new();
    private List<CardData> opponentRanged = new();
    private List<CardData> opponentSiege = new();

    // Game settings
    private int initialHandSize = 10;
    private bool randomisePlayerDeck = false;
    private int randomiseDeckSize = 25;
    private int spyDrawAmount = 2;
    private bool leaderCardsEnabled = true;
    private bool factionAbilityEnabled = true;

    private bool isPlayerTurn = true;

    // -------------------------
    // Game Initialisation
    // -------------------------

    void Start()
    {
        Debug.Log("[BoardManager] Initialising board...");
        SetGameSettings();
        SetupBoardUI();
        StartGame();
    }

    /// <summary>
    /// Set game settings and UI elements.
    /// </summary>
    private void SetGameSettings()
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
    /// Sets up the board UI with player and opponent details.
    /// </summary>
    private void SetupBoardUI()
    {
        // Setup board UI
        PlayerName.text = "Player";
        PlayerFaction.text = DeckManager.Instance.PlayerFaction;
        OpponentName.text = "NPC Opponent";
        OpponentFaction.text = DeckManager.Instance.NPCFaction;
    }

    /// <summary>
    /// Starts a new game by setting up decks, drawing hands, and initialising the turn.
    /// </summary>
    private void StartGame()
    {
        // Setup starting hands
        GenerateInitialHand(DeckManager.Instance.PlayerDeck, initialHandSize, playerHand);
        GenerateInitialHand(DeckManager.Instance.NPCDeck, initialHandSize, opponentHand);

        // Display hands
        DisplayHand(playerHand, PlayerHandRow);
        DisplayHand(opponentHand, OpponentHandRow);

        // Setup turn
        isPlayerTurn = true;

        UpdateUI();

        Debug.Log("[BoardManager] Game started. Player's turn.");
    }

    /// <summary>
    /// Randomly generates the initial hand from the given deck.
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="size"></param>
    /// <param name="hand"></param>
    private void GenerateInitialHand(List<CardData> deck, int size, List<CardData> hand)
    {
        for (int i = 0; i < size && deck.Count > 0; i++)
        {
            var card = deck[0];
            hand.Add(card);
            deck.RemoveAt(0);
        }

        CardSorter.Sort(hand);
    }

    /// <summary>
    /// Displays the given hand in the specified row.
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="row"></param>
    private void DisplayHand(List<CardData> hand, Transform row)
    {
        foreach (var cardData in hand)
        {
            CardUI cardUI = CardManager.Instance.CreateCard(cardData, row);
            RegisterCard(cardUI);
        }

        CardSorter.Sort(hand);
    }

    // -------------------------
    // Card Interaction
    // -------------------------

    /// <summary>
    /// Handles a card being played from hand to the board.
    /// </summary>
    /// <param name="cardUI"></param>
    private void HandleCardPlayed(CardUI cardUI)
    {
        if (cardUI == null || cardUI.cardData == null)
            return;

        CardData data = cardUI.cardData;

        // Determine target based on turn
        Transform targetRowContainer = GetTargetRowContainer(data, isPlayerTurn);
        List<CardData> targetRowList = GetTargetRowList(data, isPlayerTurn);
        List<CardData> handList = isPlayerTurn ? playerHand : opponentHand;

        // Remove from hand, add to row
        handList.Remove(data);
        targetRowList.Add(data);
        cardUI.transform.SetParent(targetRowContainer, false);
        CardSorter.Sort(targetRowList);

        Debug.Log($"[BoardManager] {(isPlayerTurn ? "Player" : "Opponent")} played {data.name} to {data.range} row");

        UpdateUI();
    }

    /// <summary>
    /// Gets the target row transform based on card range and player/opponent.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    private Transform GetTargetRowContainer(CardData card, bool isPlayer)
    {
        string range = card.range?.ToLower();

        switch(range)
        {
            case CardDefs.Range.Melee:
                return isPlayer ? PlayerMeleeRow : OpponentMeleeRow;
            case CardDefs.Range.Agile:
                return isPlayer ? PlayerMeleeRow : OpponentMeleeRow; // Melee as default for Agile for now
            case CardDefs.Range.Ranged:
                return isPlayer ? PlayerRangedRow : OpponentRangedRow;
            case CardDefs.Range.Siege:
                return isPlayer ? PlayerSiegeRow : OpponentSiegeRow;
            default:
                // Fallback if data is missing or invalid
                Debug.LogWarning($"[BoardManager] Unknown range '{range}' for card '{card.name}' — defaulting to melee row.");
                return isPlayer ? PlayerMeleeRow : OpponentMeleeRow;
        }
    }

    /// <summary>
    /// Gets the target list based on card range and player/opponent.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    private List<CardData> GetTargetRowList(CardData card, bool isPlayer)
    {
        string range = card.range?.ToLower();

        switch (range)
        {
            case CardDefs.Range.Melee:
                return isPlayer ? playerMelee : opponentMelee;
            case CardDefs.Range.Agile:
                return isPlayer ? playerMelee : opponentMelee; // Melee as default for Agile for now
            case CardDefs.Range.Ranged:
                return isPlayer ? playerRanged : opponentRanged;
            case CardDefs.Range.Siege:
                return isPlayer ? playerSiege : opponentSiege;
            default:
                // Fallback if data is missing or invalid
                Debug.LogWarning($"[BoardManager] Unknown range '{range}' for card '{card.name}' — defaulting to melee row.");
                return isPlayer ? playerMelee : opponentMelee;
        }
    }

    /// <summary>
    /// Sets up event handlers for the given card.
    /// </summary>
    /// <param name="card"></param>
    public void RegisterCard(CardUI card)
    {
        card.OnCardClicked += HandleCardPlayed;
        card.OnCardDragged += HandleCardPlayed;
    }

    // -------------------------
    // Turn Management
    // -------------------------

    /// <summary>
    /// Ends the current turn and switches to the other player.
    /// </summary>
    public void EndTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        Debug.Log(isPlayerTurn ? "[BoardManager] Player's turn" : "[BoardManager] Opponent's turn");

        if (!isPlayerTurn)
            StartCoroutine(HandleOpponentTurn());
    }

    /// <summary>
    /// Handles the opponent's turn logic.
    /// </summary>
    /// <returns></returns>
    private System.Collections.IEnumerator HandleOpponentTurn()
    {
        yield return new WaitForSeconds(1f); // Small delay

        if (opponentHand.Count > 0)
        {
            // Play the first card for now
            var cardData = opponentHand[0];
            CardUI cardUI = CardManager.Instance.CreateCard(cardData, OpponentHandRow);
            HandleCardPlayed(cardUI);
        }

        EndTurn();
    }

    // -------------------------
    // UI Updates
    // -------------------------

    /// <summary>
    /// Updates the UI elements to reflect current game state.
    /// </summary>
    private void UpdateUI()
    {
        CalculateScore();

        PlayerHandSize.text = playerHand.Count.ToString();
        OpponentHandSize.text = opponentHand.Count.ToString();

        PlayerDeckSize.text = DeckManager.Instance.PlayerDeck.Count.ToString();
        OpponentDeckSize.text = DeckManager.Instance.NPCDeck.Count.ToString();
    }

    /// <summary>
    /// Calculates and updates the scores for both players.
    /// </summary>
    private void CalculateScore()
    {
        int playerScore = CalculateRowScore(playerMelee) + CalculateRowScore(playerRanged) + CalculateRowScore(playerSiege);
        int opponentScore = CalculateRowScore(opponentMelee) + CalculateRowScore(opponentRanged) + CalculateRowScore(opponentSiege);

        PlayerTotalScore.text = playerScore.ToString();
        OpponentTotalScore.text = opponentScore.ToString();

        PlayerMeleeScore.text = CalculateRowScore(playerMelee).ToString();
        PlayerRangedScore.text = CalculateRowScore(playerRanged).ToString();
        PlayerSiegeScore.text = CalculateRowScore(playerSiege).ToString();

        OpponentMeleeScore.text = CalculateRowScore(opponentMelee).ToString();
        OpponentRangedScore.text = CalculateRowScore(opponentRanged).ToString();
        OpponentSiegeScore.text = CalculateRowScore(opponentSiege).ToString();

        Debug.Log($"[BoardManager] Scores = Player: {playerScore} | Opponent: {opponentScore}");
    }

    private int CalculateRowScore(List<CardData> row)
    {
        int total = 0;
        foreach (var card in row)
            total += card.strength;
        return total;
    }
}
