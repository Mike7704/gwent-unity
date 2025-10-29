using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages card abilities and their effects.
/// </summary>
public class AbilityManager
{
    private readonly BoardManager boardManager;
    private readonly BoardState state;
    private readonly CardZoneManager zoneManager;

    private HashSet<int> resolvedAbilities = new HashSet<int>(); // Track resolved abilities each turn

    private List<CardData> playerQueuedAvengers = new(); // Cards to summon at start of round
    private List<CardData> opponentQueuedAvengers = new(); // Cards to summon at start of round

    private readonly float abilityTriggerDelay = 1f;
    private readonly float cardSummonDelay = 0.3f;

    public AbilityManager(BoardState state, BoardManager boardManager, CardZoneManager zoneManager)
    {
        this.state = state;
        this.boardManager = boardManager;
        this.zoneManager = zoneManager;
    }

    /// <summary>
    /// Resolves the ability of a played card.
    /// </summary>
    public IEnumerator ResolveCard(CardData card, bool isPlayer)
    {
        // No ability to resolve
        if (card == null || string.IsNullOrEmpty(card.ability)) yield break;

        // Check if this ability has already been resolved this turn to avoid recursion
        if (resolvedAbilities.Contains(card.id)) yield break;

        resolvedAbilities.Add(card.id);

        Debug.Log($"[AbilityManager] Triggering {card.ability} ability for [{card.name}]");

        switch (card.ability)
        {
            case CardDefs.Ability.Clear:
                // Ability
                break;

            case CardDefs.Ability.Frost:
                // Ability
                break;

            case CardDefs.Ability.Fog:
                // Ability
                break;

            case CardDefs.Ability.Rain:
                // Ability
                break;

            case CardDefs.Ability.Storm:
                // Ability
                break;

            case CardDefs.Ability.Nature:
                // Ability
                break;

            case CardDefs.Ability.WhiteFrost:
                // Ability
                break;

            case CardDefs.Ability.Avenger:
                // Handled when moved to graveyard
                break;

            case CardDefs.Ability.Bond:
                // Ability
                break;

            case CardDefs.Ability.Decoy:
                // Ability
                break;

            case CardDefs.Ability.DrawEnemyDiscard:
                yield return boardManager.StartCoroutine(HandleSpy(isPlayer));
                break;

            case CardDefs.Ability.Horn:
                // Ability
                break;

            case CardDefs.Ability.Mardroeme:
                // Ability
                break;

            case CardDefs.Ability.Medic:
                // Ability
                break;

            case CardDefs.Ability.Morale:
                // Ability
                break;

            case CardDefs.Ability.Morph:
                // Ability
                break;

            case CardDefs.Ability.Muster:
                yield return boardManager.StartCoroutine(HandleMuster(card, isPlayer, isMusterPlus:false));
                break;

            case CardDefs.Ability.MusterPlus:
                yield return boardManager.StartCoroutine(HandleMuster(card, isPlayer, isMusterPlus:true));
                break;

            case CardDefs.Ability.Scorch:
                // Ability
                break;

            case CardDefs.Ability.ScorchRow:
                // Ability
                break;

            case CardDefs.Ability.Spy:
                yield return boardManager.StartCoroutine(HandleSpy(isPlayer));
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Queues the avenger target to be summoned later.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="isPlayer"></param>
    public void QueueAvenger(CardData card, bool isPlayer)
    {
        if (card.target == null || card.target.Count == 0)
        {
            Debug.LogWarning($"[AbilityManager] Avenger has no target defined for [{card.name}]");
            return;
        }

        // Find the card the avenger will summon
        List<CardData> cardToSummon = FindCardsByTargetIDs(card.target, isPlayer, searchSummonDeck:true);

        if (cardToSummon.Count == 0) return;

        foreach (var summonCard in cardToSummon)
        {
            if (isPlayer)
                playerQueuedAvengers.Add(summonCard);
            else
                opponentQueuedAvengers.Add(summonCard);
        }
    }

    /// <summary>
    /// Play all queued avengers onto the board.
    /// </summary>
    public IEnumerator ResolveQueuedAvengers()
    {
        if (playerQueuedAvengers.Count == 0 && opponentQueuedAvengers.Count == 0) yield break;

        Debug.Log($"[AbilityManager] Resolving avenger cards...");

        AudioSystem.Instance.PlaySFX(SFX.CardSummon);

        foreach (var cardToSummon in playerQueuedAvengers)
        {
            yield return new WaitForSeconds(cardSummonDelay);
            zoneManager.AddCardToBoard(cardToSummon, isPlayer: true);
            yield return boardManager.StartCoroutine(ResolveCard(cardToSummon, isPlayer: true));
        }

        foreach (var cardToSummon in opponentQueuedAvengers)
        {
            yield return new WaitForSeconds(cardSummonDelay);
            zoneManager.AddCardToBoard(cardToSummon, isPlayer: false);
            yield return boardManager.StartCoroutine(ResolveCard(cardToSummon, isPlayer: false));
        }

        playerQueuedAvengers.Clear();
        opponentQueuedAvengers.Clear();
    }

    /// <summary>
    /// Spy ability: The player draws cards from their deck.
    /// </summary>
    private IEnumerator HandleSpy(bool isPlayer)
    {
        List<CardData> targetDeck = isPlayer ? state.playerDeck : state.opponentDeck;

        if (targetDeck.Count == 0) yield break;

        yield return new WaitForSeconds(abilityTriggerDelay);

        AudioSystem.Instance.PlaySFX(SFX.CardSpy);

        for (int i = 0; i < boardManager.spyDrawAmount; i++)
        {
            if (targetDeck.Count > 0)
            {
                int randomIndex = RandomUtils.GetRandom(0, targetDeck.Count - 1);
                CardData cardToDraw = targetDeck[randomIndex];
                zoneManager.AddCardToHand(cardToDraw, isPlayer);
            }
        }
    }

    /// <summary>
    /// Muster ability: Summons all targeted cards from the hand and deck (or summon deck if muster plus) to the board.
    /// </summary>
    /// <param name="isPlayer"></param>
    private IEnumerator HandleMuster(CardData card, bool isPlayer, bool isMusterPlus)
    {
        if (card.target == null || card.target.Count == 0)
        {
            Debug.LogWarning($"[AbilityManager] Muster ability triggered but no targets defined for [{card.name}]");
            yield break;
        }

        // Find all valid muster targets
        List<CardData> cardsToSummon = FindCardsByTargetIDs(card.target, isPlayer, isMusterPlus);

        if (cardsToSummon.Count == 0) yield break;

        yield return new WaitForSeconds(abilityTriggerDelay);

        AudioSystem.Instance.PlaySFX(SFX.CardSummon);

        // Summon each card
        foreach (var summonCard in cardsToSummon)
        {
            zoneManager.AddCardToBoard(summonCard, isPlayer);
            yield return boardManager.StartCoroutine(ResolveCard(summonCard, isPlayer));
            yield return new WaitForSeconds(cardSummonDelay);
        }
    }

    /// <summary>
    /// Returns cards based on target IDs for abilities like Muster.
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    private List<CardData> FindCardsByTargetIDs(List<CardTarget> targets, bool isPlayer, bool searchSummonDeck)
    {
        List<CardData> result = new List<CardData>();

        if (searchSummonDeck)
        {
            // Search only the summon deck for each target ID
            List<CardData> summonDeck = isPlayer ? state.playerSummonDeck : state.opponentSummonDeck;

            foreach (var target in targets)
            {
                // Player summon cards
                result.AddRange(summonDeck.Where(c => c.id == target.id));

                // Opponent summon cards (with 1000 offset)
                result.AddRange(summonDeck.Where(c => c.id - 1000 == target.id));
            }
        }
        else
        {
            // Search hand and deck for each target ID
            List<CardData> deck = isPlayer ? state.playerDeck : state.opponentDeck;
            List<CardData> hand = isPlayer ? state.playerHand : state.opponentHand;

            foreach (var target in targets)
            {
                // Player cards
                result.AddRange(hand.Where(c => c.id == target.id));
                result.AddRange(deck.Where(c => c.id == target.id));

                // Opponent cards (with 1000 offset)
                result.AddRange(hand.Where(c => c.id - 1000 == target.id));
                result.AddRange(deck.Where(c => c.id - 1000 == target.id));
            }
        }

        // Remove any duplicates
        return result.Distinct().ToList();
    }

    /// <summary>
    /// Resets the resolved abilities tracker - should be called before a new ability resolution phase.
    /// </summary>
    public void ResetResolvedAbilities()
    {
        resolvedAbilities.Clear();
    }
}
