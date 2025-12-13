using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Handles opponent logic for choosing and playing cards.
/// </summary>
public class AIOpponent
{
    // Deck state
    private List<CardData> hand = new List<CardData>();
    private List<CardData> deck = new List<CardData>();
    private List<CardData> graveyard = new List<CardData>();
    private List<CardData> cardsOnBoard = new List<CardData>();

    // Card selection
    private List<CardOption> cardOptions = new();
    public CardData cardToPlay;
    public CardData cardToTarget;

    // Ability flags
    private bool hasCardClearWeather;
    private bool hasCardFrost;
    private bool hasCardFog;
    private bool hasCardRain;
    private bool hasCardStorm;
    private bool hasCardNature;
    private bool hasCardWhiteFrost;
    private bool hasCardMardroeme;
    private bool hasCardDecoy;
    private bool hasCardSpy;
    private bool hasCardMedic;
    private bool hasCardScorch;
    private bool hasCardScorchRowMelee;
    private bool hasCardScorchRowRanged;
    private bool hasCardScorchRowSiege;
    private bool hasCardHorn;

    // Score tracking
    private int totalScore;
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
            cardToPlay = GetRandomCard(hand);
            Debug.Log($"[AIOpponent] Selected random card: [{cardToPlay.name}]");
        }
    }

    /// <summary>
    /// Reads the current board state and updates variables.
    /// </summary>
    private void ReadBoard()
    {
        // Update hand, deck, graveyard, and board cards
        hand = new List<CardData>(state.opponentHand);
        deck = new List<CardData>(state.opponentDeck);
        graveyard = new List<CardData>(state.opponentGraveyard);
        cardsOnBoard = new List<CardData>();
        cardsOnBoard.AddRange(state.opponentMelee);
        cardsOnBoard.AddRange(state.opponentRanged);
        cardsOnBoard.AddRange(state.opponentSiege);

        // Reset selections
        cardOptions.Clear();
        cardToPlay = null;
        cardToTarget = null;

        // Include leader card in hand for ability checks
        if (state.opponentLeader.Any())
            hand.Add(state.opponentLeader[0]);

        // Ability checks
        hasCardClearWeather = hand.Any(card => card.ability == CardDefs.Ability.Clear);
        hasCardFrost = hand.Any(card => card.ability == CardDefs.Ability.Frost);
        hasCardFog = hand.Any(card => card.ability == CardDefs.Ability.Fog);
        hasCardRain = hand.Any(card => card.ability == CardDefs.Ability.Rain);
        hasCardStorm = hand.Any(card => card.ability == CardDefs.Ability.Storm);
        hasCardNature = hand.Any(card => card.ability == CardDefs.Ability.Nature);
        hasCardWhiteFrost = hand.Any(card => card.ability == CardDefs.Ability.WhiteFrost);
        hasCardMardroeme = hand.Any(card => card.ability == CardDefs.Ability.Mardroeme);
        hasCardDecoy = hand.Any(card => card.ability == CardDefs.Ability.Decoy);
        hasCardSpy = hand.Any(card => card.ability == CardDefs.Ability.Spy);
        hasCardMedic = hand.Any(card => card.ability == CardDefs.Ability.Medic);
        hasCardScorch = hand.Any(card => card.ability == CardDefs.Ability.Scorch);
        hasCardScorchRowMelee = hand.Any(card => card.ability == CardDefs.Ability.ScorchRow && card.range == CardDefs.Range.Melee);
        hasCardScorchRowRanged = hand.Any(card => card.ability == CardDefs.Ability.ScorchRow && card.range == CardDefs.Range.Ranged);
        hasCardScorchRowSiege = hand.Any(card => card.ability == CardDefs.Ability.ScorchRow && card.range == CardDefs.Range.Siege);
        hasCardHorn = hand.Any(card => card.ability == CardDefs.Ability.Horn);

        // Score calculation
        totalScore = state.GetOpponentTotalScore();
        totalPlayerScore = state.GetPlayerTotalScore();

        // Board row checks
        isStandardSpyCardOnBoard = HasTypeWithAbility(cardsOnBoard, CardDefs.Type.Standard, CardDefs.Ability.Spy);
        isStandardMedicCardOnBoard = HasTypeWithAbility(cardsOnBoard, CardDefs.Type.Standard, CardDefs.Ability.Medic);
        isStandardScorchRowCardOnBoard = HasTypeWithAbility(cardsOnBoard, CardDefs.Type.Standard, CardDefs.Ability.ScorchRow);

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
        if (hand.Count == 0)
            return true;

        // Player has passed and we are winning
        if (state.PlayerHasPassed && (totalScore > totalPlayerScore || (totalScore == totalPlayerScore && canWinDraws)))
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
        Debug.Log("[AIOpponent] Evaluating Decoy options...");

        // Check if we have a decoy card to play
        if (!hasCardDecoy)
            return;

        // Decoy cards are the same, so just get the first one
        CardData decoyCard = GetCardsWithAbility(hand, CardDefs.Ability.Decoy)[0];

        // Select a decoy if a spy is on the board
        if (isStandardSpyCardOnBoard && deck.Count > 0)
        {
            int score = 100;
            string reason = "Decoy a spy to draw more cards";
            CardData cardToDecoy = GetCardToDecoy(CardDefs.Ability.Spy);
            cardOptions.Add(new CardOption(decoyCard, score, reason, cardToDecoy));
        }

        // Select a decoy if a medic is on the board
        if (isStandardMedicCardOnBoard && graveyard.Count > 0)
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
        if (totalScore <= totalPlayerScore && hand.Count < 4 && state.OpponentLife == 2)
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
        Debug.Log("[AIOpponent] Evaluating Spy options...");

        // Check if we have a spy card to play and cards in deck to draw
        if (!hasCardSpy || deck.Count == 0)
            return;

        List<CardData> spyCards = GetCardsWithAbility(hand, CardDefs.Ability.Spy);
        cardOptions.Add(new CardOption(GetRandomCard(spyCards), 100, "Spy to draw more cards"));
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
        if (cardsOnBoard == null || cardsOnBoard.Count == 0)
            return null;

        List<CardData> cardsWithAbility;

        // If no ability specified, pick any card
        if (ability == null)
            cardsWithAbility = cardsOnBoard;
        else
            cardsWithAbility = GetCardsWithAbility(cardsOnBoard, ability);

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