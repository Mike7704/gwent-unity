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
    private List<CardData> hand = new List<CardData>();
    private List<CardData> deck = new List<CardData>();
    private List<CardData> graveyard = new List<CardData>();
    private CardData cardToPlay;

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
        ChooseSpy();

        if (cardToPlay == null)
        {
            cardToPlay = GetRandomCard(hand);
            Debug.Log($"[AIOpponent] Selected random: [{cardToPlay.name}]");
        }

        boardManager.HandleCardPlayed(cardToPlay, isPlayer: false);
    }

    /// <summary>
    /// Reads the current board state and updates variables.
    /// </summary>
    private void ReadBoard()
    {
        hand = state.opponentHand;
        deck = state.opponentDeck;
        graveyard = state.opponentGraveyard;
        cardToPlay = null;

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

    private void ChooseSpy()
    {
        Debug.Log("[AIOpponent] Evaluating spy options...");

        // Check if we have a spy card to play
        if (!hasCardSpy)
            return;

        // Check there are cards we can recover
        if (deck.Count == 0)
            return;

        List<CardData> spyCards = GetCardsWithAbility(CardDefs.Ability.Spy);
        cardToPlay = GetRandomCard(spyCards);

        Debug.Log($"[AIOpponent] Selected Spy: [{cardToPlay.name}]");
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
    /// Gets all cards in hand with a specific ability.
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    private List<CardData> GetCardsWithAbility(string ability)
    {
        return hand.Where(c => c.ability == ability).ToList();
    }



}
