using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles opponent logic for choosing and playing cards.
/// </summary>
public class AIOpponent
{
    // Deck state
    private List<CardData> npcHand = new List<CardData>();
    private List<CardData> npcDeck = new List<CardData>();
    private List<CardData> npcGraveyard = new List<CardData>();
    private List<CardData> npcCardsOnBoard = new List<CardData>();
    private List<CardData> playerCardsOnBoard = new List<CardData>();

    // Card selection
    private List<CardOption> cardOptions = new();
    public CardData cardToPlay;
    public CardData cardToTarget;

    // Score tracking
    private int totalNPCScore;
    private int totalPlayerScore;

    // Row tracking
    private bool isStandardSpyCardOnBoard;
    private bool isStandardMedicCardOnBoard;
    private bool isStandardScorchRowCardOnBoard;

    // Faction ability
    private bool canWinDraws;

    private readonly BoardState state;
    private readonly BoardManager boardManager;

    public AIOpponent(BoardState state, BoardManager boardManager)
    {
        this.state = state;
        this.boardManager = boardManager;
    }

    /// <summary>
    /// Executes the opponent's turn.
    /// </summary>
    public IEnumerator PlayTurn()
    {
        if (state.IsPlayerTurn)
        {
            Debug.Log("[AIOpponent] It's the player's turn! Cannot play.");
            yield break;
        }

        // Update internal state based on the current board
        ReadBoard();

        // Pass this turn?
        if (ShouldPass())
        {
            Debug.Log("[AIOpponent] Passing turn...");
            boardManager.PassRound(isPlayer: false);
            yield break;
        }

        // Check ability options
        ChooseDecoy();
        ChooseSpy();
        ChooseMedic();
        ChooseAvenger();
        ChooseScorch();

        // Now choose the best card to play
        GetBestCardOption();

        // Play the chosen card
        boardManager.HandleCardPlayed(cardToPlay, isPlayer: false);
    }

    /// <summary>
    /// Chooses the best card option based on scores/impact.
    /// </summary>
    private void GetBestCardOption()
    {
        if (cardOptions.Count > 0)
        {
            // Get the highest scoring card option
            int bestScore = cardOptions.Max(c => c.score);

            // Get all options with the best score
            List<CardOption> bestOptions = cardOptions.Where(c => c.score == bestScore).ToList();

            CardOption chosenCard = bestOptions[RandomUtils.GetRandom(0, bestOptions.Count - 1)]; ;

            cardToPlay = chosenCard.card;

            // Check if card has a target to select after (Decoy)
            if (chosenCard.targetCard != null)
                cardToTarget = chosenCard.targetCard;

            Debug.Log($"[AIOpponent] Selected [{cardToPlay.name}] | Score {chosenCard.score} | {chosenCard.reason}");
        }
        else
        {
            // Play a random card if no options available
            cardToPlay = GetRandomCard(npcHand);
            Debug.Log($"[AIOpponent] Selected random card: [{cardToPlay.name}]");
        }
    }

    /// <summary>
    /// Reads the current board state and updates variables.
    /// </summary>
    private void ReadBoard()
    {
        // Update hand, deck, graveyard, and board cards
        npcHand = new List<CardData>(state.opponentHand);
        npcDeck = new List<CardData>(state.opponentDeck);
        npcGraveyard = new List<CardData>(state.opponentGraveyard);
        npcCardsOnBoard = new List<CardData>();
        npcCardsOnBoard.AddRange(state.opponentMelee);
        npcCardsOnBoard.AddRange(state.opponentRanged);
        npcCardsOnBoard.AddRange(state.opponentSiege);
        playerCardsOnBoard = new List<CardData>();
        playerCardsOnBoard.AddRange(state.playerMelee);
        playerCardsOnBoard.AddRange(state.playerRanged);
        playerCardsOnBoard.AddRange(state.playerSiege);

        // Reset selections
        cardOptions.Clear();
        cardToPlay = null;
        cardToTarget = null;

        // Include leader card in hand for ability checks
        if (state.opponentLeader.Any())
            npcHand.Add(state.opponentLeader[0]);

        // Score calculation
        totalNPCScore = state.GetOpponentTotalScore();
        totalPlayerScore = state.GetPlayerTotalScore();

        // Board row checks
        isStandardSpyCardOnBoard = HasTypeWithAbility(npcCardsOnBoard, CardDefs.Type.Standard, CardDefs.Ability.Spy);
        isStandardMedicCardOnBoard = HasTypeWithAbility(npcCardsOnBoard, CardDefs.Type.Standard, CardDefs.Ability.Medic);
        isStandardScorchRowCardOnBoard = HasTypeWithAbility(npcCardsOnBoard, CardDefs.Type.Standard, CardDefs.Ability.ScorchRow);

        // Faction ability check (Nilfgaard)
        canWinDraws = boardManager.playerFaction != CardDefs.Faction.Nilfgaard && boardManager.opponentFaction == CardDefs.Faction.Nilfgaard;
    }

    /// <summary>
    /// Evaluates whether the AI should pass its turn.
    /// </summary>
    /// <returns></returns>
    private bool ShouldPass()
    {
        Debug.Log("[AIOpponent] Evaluating pass options...");

        // No cards left to play
        if (npcHand.Count == 0)
            return true;

        // Player has passed and we are winning
        if (state.PlayerHasPassed && (totalNPCScore > totalPlayerScore || (totalNPCScore == totalPlayerScore && canWinDraws)))
            return true;

        return false;
    }

    // -------------------------
    // Ability Choice Functions
    // -------------------------

    /// <summary>
    /// Check if a Decoy card should be played.
    /// </summary>
    private void ChooseDecoy()
    {
        CardData decoyCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.Decoy));

        // Check if we have a decoy card to play
        if (decoyCard == null)
            return;

        Debug.Log("[AIOpponent] Evaluating Decoy options...");

        // Select a decoy if a spy is on the board
        if (isStandardSpyCardOnBoard && npcDeck.Count > 0)
        {
            int score = 100;
            string reason = "Decoy a spy to draw more cards";
            CardData cardToDecoy = GetCardToDecoy(CardDefs.Ability.Spy);
            cardOptions.Add(new CardOption(decoyCard, score, reason, cardToDecoy));
        }

        // Select a decoy if a medic is on the board
        if (isStandardMedicCardOnBoard && npcGraveyard.Count > 0)
        {
            int score = 80;
            string reason = "Decoy a medic to play more cards";
            CardData cardToDecoy = GetCardToDecoy(CardDefs.Ability.Medic);
            cardOptions.Add(new CardOption(decoyCard, score, reason, cardToDecoy));
        }

        // Select a decoy if a scorch row is on the board
        if (isStandardScorchRowCardOnBoard)
        {
            int score = 50;
            string reason = "Decoy a scorch row to destroy player cards";
            CardData cardToDecoy = GetCardToDecoy(CardDefs.Ability.ScorchRow);
            cardOptions.Add(new CardOption(decoyCard, score, reason, cardToDecoy));
        }

        // Low on cards, time to use a decoy on any card
        if (totalNPCScore <= totalPlayerScore && npcHand.Count < 4 && state.OpponentLife == 2)
        {
            int score = 30;
            string reason = "Decoy a random card to use a turn";
            CardData cardToDecoy = GetCardToDecoy(null);
            cardOptions.Add(new CardOption(decoyCard, score, reason, cardToDecoy));
        }
    }

    /// <summary>
    /// Check if a Spy card should be played.
    /// </summary>
    private void ChooseSpy()
    {
        List<CardData> spyCards = GetCardsWithAbility(npcHand, CardDefs.Ability.Spy);

        // Check if we have a spy card to play and cards in deck to draw
        if (spyCards == null || spyCards.Count == 0 || npcDeck.Count == 0)
            return;

        Debug.Log("[AIOpponent] Evaluating Spy options...");

        cardOptions.Add(new CardOption(GetRandomCard(spyCards), 100, "Spy to draw more cards"));
    }

    /// <summary>
    /// Check if a Medic card should be played.
    /// </summary>
    private void ChooseMedic()
    {
        CardData medicCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.Medic));

        // Check if we have a medic card to play
        if (medicCard == null)
            return;

        Debug.Log("[AIOpponent] Evaluating Medic options...");

        bool isSpyInGraveyard = HasTypeWithAbility(npcGraveyard, CardDefs.Type.Standard, CardDefs.Ability.Spy);
        bool isMedicInGraveyard = HasTypeWithAbility(npcGraveyard, CardDefs.Type.Standard, CardDefs.Ability.Medic);

        // Select a medic to recover a spy card
        if (isSpyInGraveyard && npcDeck.Count > 0)
        {
            int score = 60;
            string reason = "Medic a spy to gain more cards";
            CardData cardToMedic = GetCardToMedic(CardDefs.Ability.Spy);
            cardOptions.Add(new CardOption(medicCard, score, reason, cardToMedic));
        }

        // Select a medic to recover a medic card
        if (isMedicInGraveyard)
        {
            int score = (isSpyInGraveyard) ? 80 : 30; // Higher score if a spy to recover after using the medic
            string reason = "Medic a medic to recover an extra card";
            CardData cardToMedic = GetCardToMedic(CardDefs.Ability.Medic);
            cardOptions.Add(new CardOption(medicCard, score, reason, cardToMedic));
        }

        // Select a medic for the score (we have no cards in graveyard)
        if (npcGraveyard.Count == 0 && npcHand.Count < 4)
        {
            int score = 10;
            string reason = "Medic highest strength for the score";
            CardData cardToMedic = GetCardToMedic(null);
            cardOptions.Add(new CardOption(medicCard, score, reason, cardToMedic));
        }
    }

    /// <summary>
    /// Check if an Avenger card should be played.
    /// </summary>
    private void ChooseAvenger()
    {
        List<CardData> avengerCards = GetCardsWithAbility(npcHand, CardDefs.Ability.Avenger);

        // Check if we have an avenger card to play
        if (avengerCards == null || avengerCards.Count == 0)
            return;

        Debug.Log("[AIOpponent] Evaluating Avenger options...");

        cardOptions.Add(new CardOption(GetRandomCard(avengerCards), 50, "Avenger to have for the next round"));
    }

    /// <summary>
    /// Check if a Scorch card should be played.
    /// </summary>
    private void ChooseScorch()
    {
        List<CardData> scorchCards = GetCardsWithAbility(npcHand, CardDefs.Ability.Scorch);

        // Check if we have a scorch card to play
        if (scorchCards == null || scorchCards.Count == 0)
            return;

        Debug.Log("[AIOpponent] Evaluating Scorch options...");

        // Find the highest strength cards on both sides
        int highestPlayerCardStrength = playerCardsOnBoard
            .Where(card => card.type == CardDefs.Type.Standard)
            .Select(card => card.strength)
            .DefaultIfEmpty(0)
            .Max();
        int highestNPCCardStrength = npcCardsOnBoard
            .Where(card => card.type == CardDefs.Type.Standard)
            .Select(card => card.strength)
            .DefaultIfEmpty(0)
            .Max();

        // Count how many cards would be destroyed on both sides
        int playerCardsToDestroy = playerCardsOnBoard.Count(card => card.strength == highestPlayerCardStrength && card.type == CardDefs.Type.Standard);
        int npcCardsToDestroy = npcCardsOnBoard.Count(card => card.strength == highestPlayerCardStrength && card.type == CardDefs.Type.Standard);

        // Only play Scorch if it destroys more player cards than opponent cards
        if (playerCardsToDestroy > npcCardsToDestroy)
        {
            int score = ((playerCardsToDestroy * highestPlayerCardStrength) - (npcCardsToDestroy * highestPlayerCardStrength)) * 2;
            cardOptions.Add(new CardOption(GetRandomCard(scorchCards), score, "Scorch high strength player cards"));
            return;
        }
    }

    // -------------------------
    // Helper Functions
    // -------------------------

    /// <summary>
    /// Gets a random card from a list.
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    private CardData GetRandomCard(List<CardData> cards)
    {
        if (cards == null || cards.Count == 0)
            return null;

        int index = RandomUtils.GetRandom(0, cards.Count - 1);
        return cards[index];
    }

    /// <summary>
    /// Gets a random card with ability from the board to decoy.
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    private CardData GetCardToDecoy(string ability)
    {
        if (npcCardsOnBoard == null || npcCardsOnBoard.Count == 0)
            return null;

        List<CardData> cardsWithAbility;

        // If no ability specified, pick any card
        if (ability == null)
            cardsWithAbility = npcCardsOnBoard;
        else
            cardsWithAbility = GetCardsWithAbility(npcCardsOnBoard, ability);

        int index = RandomUtils.GetRandom(0, cardsWithAbility.Count - 1);
        return cardsWithAbility[index];
    }

    /// <summary>
    /// Gets a random card with ability from the graveyard to recover.
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    private CardData GetCardToMedic(string ability)
    {
        if (npcGraveyard.Count == 0)
            return null;

        // If no ability specified, pick highest strength card
        if (ability == null)
            return npcGraveyard[npcGraveyard.Count];

        List<CardData> cardsWithAbility = GetCardsWithAbility(npcGraveyard, ability);

        int index = RandomUtils.GetRandom(0, cardsWithAbility.Count - 1);
        return cardsWithAbility[index];
    }

    /// <summary>
    /// Gets all cards in zone with a specific ability.
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    private List<CardData> GetCardsWithAbility(List<CardData> zone, string ability)
    {
        return zone.Where(c => c.ability == ability).ToList();
    }

    /// <summary>
    /// Check if a specific type with ability exists in a zone.
    /// </summary>
    /// <param name="zone"></param>
    /// <param name="type"></param>
    /// <param name="ability"></param>
    /// <returns></returns>
    private bool HasTypeWithAbility(List<CardData> zone, string type, string ability)
    {
        return zone.Any(card => card.type == type && card.ability == ability);
    }

}

/// <summary>
/// Card option with score and reason for AI decision making.
/// </summary>
class CardOption
{
    public CardData card;
    public CardData targetCard; // for decoy
    public int score;
    public string reason;

    public CardOption(CardData card, int score, string reason, CardData targetCard = null)
    {
        this.card = card;
        this.score = score;
        this.reason = reason;
        this.targetCard = targetCard;
    }
}