using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages card abilities and their effects.
/// </summary>
public class AbilityManager
{
    private readonly BoardManager boardManager;
    private readonly BoardState state;
    private readonly CardZoneManager zoneManager;

    private readonly float abilityTriggerDelay = 1f;

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
        if (card == null || string.IsNullOrEmpty(card.ability)) yield break;

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
                // Ability
                break;

            case CardDefs.Ability.Bond:
                // Ability
                break;

            case CardDefs.Ability.Decoy:
                // Ability
                break;

            case CardDefs.Ability.DrawEnemyDiscard:
                // Ability
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
                // Ability
                break;

            case CardDefs.Ability.MusterPlus:
                // Ability
                break;

            case CardDefs.Ability.Scorch:
                // Ability
                break;

            case CardDefs.Ability.ScorchRow:
                // Ability
                break;

            case CardDefs.Ability.Spy:
                yield return HandleSpy(isPlayer);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Spy ability: The player draws cards from their deck.
    /// </summary>
    private IEnumerator HandleSpy(bool isPlayer)
    {
        yield return new WaitForSeconds(abilityTriggerDelay);

        AudioSystem.Instance.PlaySFX(SFX.CardSpy);

        List<CardData> targetDeck = isPlayer ? state.playerDeck : state.opponentDeck;

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

}
