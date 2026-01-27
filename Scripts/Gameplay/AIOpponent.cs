using System;
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
    private RowZone rowToTarget;

    // Score tracking
    private int totalNPCScore;
    private int totalPlayerScore;

    // Row tracking
    private bool isStandardSpyCardOnBoard;
    private bool isStandardMedicCardOnBoard;
    private bool isStandardScorchRowCardOnBoard;

    // Standard card strength calculations
    private int playerStandardMeleeStrength;
    private int playerStandardRangedStrength;
    private int playerStandardSiegeStrength;
    private int npcStandardMeleeStrength;
    private int npcStandardRangedStrength;
    private int npcStandardSiegeStrength;

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

        // Check ability options
        ChooseClearWeatherAbiltiy();
        ChooseWeatherAbility();
        ChooseDecoy();
        ChooseSpy();
        ChooseMedic();
        ChooseAvenger();
        ChooseScorch();
        ChooseScorchRow();
        ChooseHorn();
        ChooseMardroeme();

        // Now choose the best card to play
        GetBestCardOption();

        // Pass if winning, no good cards, or no cards to play
        if (ShouldPass() || cardToPlay == null)
        {
            Debug.Log("[AIOpponent] Passing turn...");
            boardManager.PassRound(isPlayer: false);
            yield break;
        }

        // Handle special card (horn/mardroeme)
        if (rowToTarget != RowZone.None)
        {
            boardManager.abilityManager.HandleSpecialCardSelection(cardToPlay, rowToTarget, isPlayer: false);
            yield break;
        }

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

            // Check if card has a target row to select (horn/mardroeme)
            if (chosenCard.targetRow != RowZone.None)
                rowToTarget = chosenCard.targetRow;

            Debug.Log($"[AIOpponent] Selected [{cardToPlay.name}] | Score {chosenCard.score} | {chosenCard.reason}");
        }
        else
        {
            // Play a random card if no options available
            List<CardData> randomOptions = new List<CardData>();
            randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.Bond));
            randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.Morale));
            randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.Muster));
            randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.MusterPlus));
            randomOptions.AddRange(GetCardsWithoutAbility(npcHand));

            if (randomOptions.Count == 0)
            {
                // Lets try more options
                randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.Morph));
                randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.Mardroeme));
                randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.Horn));
            }

            if (randomOptions.Count == 0)
            {
                // We must really be out of options
                randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.ScorchRow));
                randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.Avenger));
                randomOptions.AddRange(GetCardsWithAbility(npcHand, CardDefs.Ability.Medic));
            }

            if (randomOptions.Count > 0)
            {
                cardToPlay = GetRandomCard(randomOptions);
                Debug.Log($"[AIOpponent] Selected random card: [{cardToPlay.name}]");
            }
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
        rowToTarget = RowZone.None;

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

        // Standard card strength calculations
        playerStandardMeleeStrength = state.playerMelee.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.strength);
        playerStandardRangedStrength = state.playerRanged.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.strength);
        playerStandardSiegeStrength = state.playerSiege.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.strength);
        npcStandardMeleeStrength = state.opponentMelee.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.strength);
        npcStandardRangedStrength = state.opponentRanged.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.strength);
        npcStandardSiegeStrength = state.opponentSiege.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.strength);

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
    /// Check if a Clear Weather card should be played.
    /// </summary>
    private void ChooseClearWeatherAbiltiy()
    {
        CardData clearWeatherCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.Clear));

        // Check if we have a clear weather card to play
        if (clearWeatherCard == null)
            return;

        Debug.Log("[AIOpponent] Evaluating Clear Weather options...");

        // Check if any weather effects are active on the board
        bool isFrostActive = IsWeatherActiveOnRow(CardDefs.Range.Melee);
        bool isFogActive = IsWeatherActiveOnRow(CardDefs.Range.Ranged);
        bool isRainActive = IsWeatherActiveOnRow(CardDefs.Range.Siege);

        // Clear weather to remove negative effects and increase score
        if (isFrostActive || isFogActive || isRainActive)
        {
            int score = 0;

            // Calculate amount of strength restored if weather is cleared
            int playerRestoreMelee = state.playerMelee.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.defaultStrength - c.strength);
            int playerRestoreRanged = state.playerRanged.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.defaultStrength - c.strength);
            int playerRestoreSiege = state.playerSiege.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.defaultStrength - c.strength);

            int npcRestoreMelee = state.opponentMelee.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.defaultStrength - c.strength);
            int npcRestoreRanged = state.opponentRanged.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.defaultStrength - c.strength);
            int npcRestoreSiege = state.opponentSiege.Where(c => c.type == CardDefs.Type.Standard).Sum(c => c.defaultStrength - c.strength);

            // Check if NPC gains strength advantage on any row by clearing weather
            if (isFrostActive)
                score += (npcRestoreMelee - playerRestoreMelee);

            if (isFogActive)
                score += (npcRestoreRanged - playerRestoreRanged);

            if (isRainActive)
                score += (npcRestoreSiege - playerRestoreSiege);

            if (totalNPCScore < totalPlayerScore)
                score += 5;

            if (state.PlayerHasPassed && totalNPCScore + score > totalPlayerScore)
                score += 10;

            if (npcHand.Count < 4)
                score += 5;

            if (score > 15)
                cardOptions.Add(new CardOption(clearWeatherCard, score, "Clear Weather to increase total score"));
        }
    }

    /// <summary>
    /// Check if a Weather card should be played.
    /// </summary>
    private void ChooseWeatherAbility()
    {
        // Get weather cards in hand
        CardData frostCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.Frost));
        CardData fogCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.Fog));
        CardData rainCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.Rain));
        CardData natureCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.Nature));
        CardData stormCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.Storm));
        CardData whiteFrostCard = GetRandomCard(GetCardsWithAbility(npcHand, CardDefs.Ability.WhiteFrost));

        // Check if we have any weather card to play
        if (frostCard == null && fogCard == null && rainCard == null && natureCard == null && stormCard == null && whiteFrostCard == null)
            return;

        Debug.Log("[AIOpponent] Evaluating Weather options...");

        bool totalScoreThreshold = totalPlayerScore > 5; // Minimum total player score to consider weather

        int scoreBonus = 0;
        if (state.PlayerHasPassed)
            scoreBonus += 10; // Bonus if player has passed as the round could be won
        if (npcHand.Count < 4)
            scoreBonus += 5; // Bonus if low on cards
        if (totalNPCScore > totalPlayerScore + 15 && !state.PlayerHasPassed)
            scoreBonus -= 5; // Penalty if already winning by a lot

        // Select Frost if player melee strength is high
        if (frostCard != null && !IsWeatherActiveOnRow(CardDefs.Range.Melee) && totalScoreThreshold &&
            playerStandardMeleeStrength > npcStandardMeleeStrength)
        {
            int score = (playerStandardMeleeStrength - npcStandardMeleeStrength) + scoreBonus;

            if (score > 10)
                cardOptions.Add(new CardOption(frostCard, score, "Frost to reduce player melee strength"));
        }

        // Select Fog if player ranged strength is high
        if (fogCard != null && !IsWeatherActiveOnRow(CardDefs.Range.Ranged) && totalScoreThreshold &&
            playerStandardRangedStrength > npcStandardRangedStrength)
        {
            int score = (playerStandardRangedStrength - npcStandardRangedStrength) + scoreBonus;

            if (score > 10)
                cardOptions.Add(new CardOption(fogCard, score, "Fog to reduce player ranged strength"));
        }

        // Select Rain if player siege strength is high
        if (rainCard != null && !IsWeatherActiveOnRow(CardDefs.Range.Siege) && totalScoreThreshold &&
            playerStandardSiegeStrength > npcStandardSiegeStrength)
        {
            int score = (playerStandardSiegeStrength - npcStandardSiegeStrength) + scoreBonus;

            if (score > 10)
                cardOptions.Add(new CardOption(rainCard, score, "Rain to reduce player siege strength"));
        }

        // Select Nature if player melee and siege strength is high
        if (natureCard != null && totalScoreThreshold &&
            (!IsWeatherActiveOnRow(CardDefs.Range.Melee) || !IsWeatherActiveOnRow(CardDefs.Range.Siege)) &&
            (playerStandardMeleeStrength > npcStandardMeleeStrength ||
            playerStandardSiegeStrength > npcStandardSiegeStrength))
        {
            int score = ((playerStandardMeleeStrength + playerStandardSiegeStrength) - (npcStandardMeleeStrength + npcStandardSiegeStrength)) + scoreBonus;

            if (score > 10)
                cardOptions.Add(new CardOption(natureCard, score, "Nature to reduce player melee and siege strength"));
        }

        // Select Storm if player ranged and siege strength is high
        if (stormCard != null && totalScoreThreshold &&
            (!IsWeatherActiveOnRow(CardDefs.Range.Ranged) || !IsWeatherActiveOnRow(CardDefs.Range.Siege)) &&
            (playerStandardRangedStrength > npcStandardRangedStrength ||
            playerStandardSiegeStrength > npcStandardSiegeStrength))
        {
            int score = ((playerStandardRangedStrength + playerStandardSiegeStrength) - (npcStandardRangedStrength + npcStandardSiegeStrength)) + scoreBonus;

            if (score > 10)
                cardOptions.Add(new CardOption(stormCard, score, "Storm to reduce player ranged and siege strength"));
        }

        // Select White Frost if player melee and ranged strength is high
        if (whiteFrostCard != null && totalScoreThreshold &&
            (!IsWeatherActiveOnRow(CardDefs.Range.Melee) || !IsWeatherActiveOnRow(CardDefs.Range.Ranged)) &&
            (playerStandardMeleeStrength > npcStandardMeleeStrength ||
            playerStandardRangedStrength > npcStandardRangedStrength))
        {
            int score = ((playerStandardMeleeStrength + playerStandardRangedStrength) - (npcStandardMeleeStrength + npcStandardRangedStrength)) + scoreBonus;

            if (score > 10)
                cardOptions.Add(new CardOption(whiteFrostCard, score, "White Frost to reduce player melee and ranged strength"));
        }
    }

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

        int highestCardStrength = Mathf.Max(highestPlayerCardStrength, highestNPCCardStrength);

        // Count how many cards would be destroyed on both sides
        int playerCardsToDestroy = playerCardsOnBoard.Count(card => card.strength == highestCardStrength && card.type == CardDefs.Type.Standard);
        int npcCardsToDestroy = npcCardsOnBoard.Count(card => card.strength == highestCardStrength && card.type == CardDefs.Type.Standard);

        // Only play Scorch if it destroys more player cards than opponent cards
        if (playerCardsToDestroy > npcCardsToDestroy)
        {
            int score = (playerCardsToDestroy - npcCardsToDestroy) * highestCardStrength * 2;

            // Only scorch if score is significant or we are low on cards
            if (score >= 30 || npcHand.Count < 6)
                cardOptions.Add(new CardOption(GetRandomCard(scorchCards), score, "Scorch high strength player cards"));
        }
    }

    /// <summary>
    /// Check if a Scorch Row card should be played.
    /// </summary>
    private void ChooseScorchRow()
    {
        List<CardData> scorchRowCards = GetCardsWithAbility(npcHand, CardDefs.Ability.ScorchRow);
        if (scorchRowCards == null || scorchRowCards.Count == 0)
            return;

        // Evaluate each row for playing a Scorch Row
        EvaluateScorchRow(CardDefs.Range.Melee, state.playerMelee, scorchRowCards);
        EvaluateScorchRow(CardDefs.Range.Ranged, state.playerRanged, scorchRowCards);
        EvaluateScorchRow(CardDefs.Range.Siege, state.playerSiege, scorchRowCards);
    }

    /// <summary>
    /// Calculates the score for playing a Scorch Row card on a specific row.
    /// </summary>
    /// <param name="range"></param>
    /// <param name="playerRow"></param>
    /// <param name="scorchRowCards"></param>
    private void EvaluateScorchRow(string range, List<CardData> playerRow, List<CardData> scorchRowCards)
    {
        // Check if the player row has a strength of 10 or more
        if (playerRow.Count == 0 || playerRow.Sum(c => c.strength) < 10)
            return;

        CardData scorchCard = GetRandomCard(scorchRowCards.Where(c => c.range == range).ToList());

        if (scorchCard == null)
            return;

        Debug.Log($"[AIOpponent] Evaluating Scorch Row {range} options...");

        int highestStrength = playerRow
            .Where(card => card.type == CardDefs.Type.Standard)
            .Select(card => card.strength)
            .DefaultIfEmpty(0)
            .Max();

        if (highestStrength == 0)
            return;

        // Count how many cards would be destroyed on the player side
        int cardsToDestroy = playerRow.Count(card => card.strength == highestStrength && card.type == CardDefs.Type.Standard);

        int score = cardsToDestroy * highestStrength * 2;

        // Only scorch if score is significant or we are low on cards
        if (score >= 30 || (score >= 20 && npcHand.Count < 5))
        {
            cardOptions.Add( new CardOption(scorchCard, score, $"Scorch Row {range} high strength player cards"));
        }
    }

    /// <summary>
    /// Check if a Horn card should be played.
    /// </summary>
    private void ChooseHorn()
    {
        List<CardData> hornCards = GetCardsWithAbility(npcHand, CardDefs.Ability.Horn);

        // Check if we have a horn card to play
        if (hornCards == null || hornCards.Count == 0)
            return;

        Debug.Log("[AIOpponent] Evaluating Horn options...");

        // Get horn cards by range
        CardData meleeHornCard = GetRandomCard(hornCards.Where(c => c.range == CardDefs.Range.Melee).ToList());
        CardData agileHornCard = GetRandomCard(hornCards.Where(c => c.range == CardDefs.Range.Agile).ToList());
        CardData rangedHornCard = GetRandomCard(hornCards.Where(c => c.range == CardDefs.Range.Ranged).ToList());
        CardData siegeHornCard = GetRandomCard(hornCards.Where(c => c.range == CardDefs.Range.Siege).ToList());
        CardData specialHornCard = GetRandomCard(hornCards.Where(c => c.type == CardDefs.Type.Special).ToList());

        // Check if horn is already active on rows
        bool isHornActiveOnMelee = state.opponentMeleeSpecial.Any(c => c.ability == CardDefs.Ability.Horn);
        bool isHornActiveOnRanged = state.opponentRangedSpecial.Any(c => c.ability == CardDefs.Ability.Horn);
        bool isHornActiveOnSiege = state.opponentSiegeSpecial.Any(c => c.ability == CardDefs.Ability.Horn);

        // Check if special rows are empty so we can play a special horn card
        bool isMeleeSpecialEmpty = state.opponentMeleeSpecial.Count == 0;
        bool isRangedSpecialEmpty = state.opponentRangedSpecial.Count == 0;
        bool isSiegeSpecialEmpty = state.opponentSiegeSpecial.Count == 0;

        // Check if horn should be played on rows
        int hornGainThreshold = (npcHand.Count < 4) ? 0 : 15;
        bool shouldHornMeleeRow = CalculateHornRowStrength(state.opponentMelee) >= npcStandardMeleeStrength && npcStandardMeleeStrength >= hornGainThreshold;
        bool shouldHornRangedRow = CalculateHornRowStrength(state.opponentRanged) >= npcStandardRangedStrength && npcStandardRangedStrength >= hornGainThreshold;
        bool shouldHornSiegeRow = CalculateHornRowStrength(state.opponentSiege) >= npcStandardSiegeStrength && npcStandardSiegeStrength >= hornGainThreshold;

        // Want to avoid a horn unit card from blocking a special mardroeme card (if we have morph cards)
        bool hasMardroemeSpecialCard = HasTypeWithAbility(npcHand, CardDefs.Type.Special, CardDefs.Ability.Mardroeme);
        bool shouldMardroemeMeleeFirst = hasMardroemeSpecialCard && state.opponentMelee.Any(c => c.ability == CardDefs.Ability.Morph);
        bool shouldMardroemeRangedFirst = hasMardroemeSpecialCard && state.opponentRanged.Any(c => c.ability == CardDefs.Ability.Morph);
        bool shouldMardroemeSiegeFirst = hasMardroemeSpecialCard && state.opponentSiege.Any(c => c.ability == CardDefs.Ability.Morph);

        // Evaluate horn options
        if (meleeHornCard != null && !isHornActiveOnMelee && shouldHornMeleeRow && !shouldMardroemeMeleeFirst)
            cardOptions.Add(new CardOption(meleeHornCard, npcStandardMeleeStrength, "Horn to increase melee row strength"));

        if (rangedHornCard != null && !isHornActiveOnRanged && shouldHornRangedRow && !shouldMardroemeRangedFirst)
            cardOptions.Add(new CardOption(rangedHornCard, npcStandardRangedStrength, "Horn to increase ranged row strength"));

        if (siegeHornCard != null && !isHornActiveOnSiege && shouldHornSiegeRow && !shouldMardroemeSiegeFirst)
            cardOptions.Add(new CardOption(siegeHornCard, npcStandardSiegeStrength, "Horn to increase siege row strength"));

        // Need to decide which row to add the agile horn card
        if (agileHornCard != null)
        {
            if (!isHornActiveOnMelee && shouldHornMeleeRow && !shouldMardroemeMeleeFirst && meleeHornCard == null && npcStandardMeleeStrength >= npcStandardRangedStrength)
            {
                cardOptions.Add(new CardOption(agileHornCard, npcStandardMeleeStrength, "Horn to increase melee row strength"));
            }
            else if (!isHornActiveOnRanged && shouldHornRangedRow && !shouldMardroemeRangedFirst)
            {
                cardOptions.Add(new CardOption(agileHornCard, npcStandardRangedStrength, "Horn to increase ranged row strength"));
            }
        }

        // Need to decide which row to add the special horn card
        if (specialHornCard != null)
        {
            if (!isHornActiveOnMelee && shouldHornMeleeRow && isMeleeSpecialEmpty &&
                (npcStandardMeleeStrength >= npcStandardRangedStrength || !isRangedSpecialEmpty) &&
                (npcStandardMeleeStrength >= npcStandardSiegeStrength || !isSiegeSpecialEmpty))
            {
                cardOptions.Add(new CardOption(specialHornCard, npcStandardMeleeStrength - 1, "Horn to increase melee row strength", null, RowZone.OpponentMeleeSpecial));
            }
            else if (!isHornActiveOnRanged && shouldHornRangedRow && isRangedSpecialEmpty &&
                (npcStandardRangedStrength >= npcStandardMeleeStrength || !isMeleeSpecialEmpty) &&
                (npcStandardRangedStrength >= npcStandardSiegeStrength || !isSiegeSpecialEmpty))
            {
                cardOptions.Add(new CardOption(specialHornCard, npcStandardRangedStrength - 1, "Horn to increase ranged row strength", null, RowZone.OpponentRangedSpecial));
            }
            else if (!isHornActiveOnSiege && shouldHornSiegeRow && isSiegeSpecialEmpty &&
                (npcStandardSiegeStrength >= npcStandardMeleeStrength || !isMeleeSpecialEmpty) &&
                (npcStandardSiegeStrength >= npcStandardRangedStrength || !isRangedSpecialEmpty))
            {
                cardOptions.Add(new CardOption(specialHornCard, npcStandardSiegeStrength - 1, "Horn to increase siege row strength", null, RowZone.OpponentSiegeSpecial));
            }
        }
    }

    /// <summary>
    /// Check if a Mardroeme card should be played.
    /// </summary>
    private void ChooseMardroeme()
    {
        List<CardData> mardroemeCards = GetCardsWithAbility(npcHand, CardDefs.Ability.Mardroeme);

        // Check if we have a mardroeme card to play
        if (mardroemeCards == null || mardroemeCards.Count == 0)
            return;

        Debug.Log("[AIOpponent] Evaluating Mardroeme options...");

        // Get mardroeme cards by range
        CardData meleeMardroemeCard = GetRandomCard(mardroemeCards.Where(c => c.range == CardDefs.Range.Melee).ToList());
        CardData agileMardroemeCard = GetRandomCard(mardroemeCards.Where(c => c.range == CardDefs.Range.Agile).ToList());
        CardData rangedMardroemeCard = GetRandomCard(mardroemeCards.Where(c => c.range == CardDefs.Range.Ranged).ToList());
        CardData siegeMardroemeCard = GetRandomCard(mardroemeCards.Where(c => c.range == CardDefs.Range.Siege).ToList());
        CardData specialMardroemeCard = GetRandomCard(mardroemeCards.Where(c => c.type == CardDefs.Type.Special).ToList());

        // Check if mardroeme is already active on rows
        bool isMardroemeActiveOnMelee = state.opponentMeleeSpecial.Any(c => c.ability == CardDefs.Ability.Mardroeme);
        bool isMardroemeActiveOnRanged = state.opponentRangedSpecial.Any(c => c.ability == CardDefs.Ability.Mardroeme);
        bool isMardroemeActiveOnSiege = state.opponentSiegeSpecial.Any(c => c.ability == CardDefs.Ability.Mardroeme);

        // Check if special rows are empty so we can play a special mardroeme card
        bool isMeleeSpecialEmpty = state.opponentMeleeSpecial.Count == 0;
        bool isRangedSpecialEmpty = state.opponentRangedSpecial.Count == 0;
        bool isSiegeSpecialEmpty = state.opponentSiegeSpecial.Count == 0;

        // Check if a morph card is on a row to be transformed
        bool shouldMardroemeMeleeRow = state.opponentMelee.Any(c => c.ability == CardDefs.Ability.Morph) || npcHand.Count < 5;
        bool shouldMardroemeRangedRow = state.opponentRanged.Any(c => c.ability == CardDefs.Ability.Morph) || npcHand.Count < 5;
        bool shouldMardroemeSiegeRow = state.opponentSiege.Any(c => c.ability == CardDefs.Ability.Morph) || npcHand.Count < 5;

        // Want to avoid a mardroeme unit card from blocking a special horn card
        bool hasHornSpecialCard = HasTypeWithAbility(npcHand, CardDefs.Type.Special, CardDefs.Ability.Horn);

        int score = 40;

        // Evaluate mardroeme options
        if (meleeMardroemeCard != null && !isMardroemeActiveOnMelee && shouldMardroemeMeleeRow && !hasHornSpecialCard)
            cardOptions.Add(new CardOption(meleeMardroemeCard, score, "Mardroeme to transform morph cards on melee row"));

        if (rangedMardroemeCard != null && !isMardroemeActiveOnRanged && shouldMardroemeRangedRow && !hasHornSpecialCard)
            cardOptions.Add(new CardOption(rangedMardroemeCard, score, "Mardroeme to transform morph cards on ranged row"));

        if (siegeMardroemeCard != null && !isMardroemeActiveOnSiege && shouldMardroemeSiegeRow && !hasHornSpecialCard)
            cardOptions.Add(new CardOption(siegeMardroemeCard, score, "Mardroeme to transform morph cards on siege row"));

        // Need to decide which row to add the agile mardroeme card
        if (agileMardroemeCard != null)
        {
            if (!isMardroemeActiveOnMelee && shouldMardroemeMeleeRow && !hasHornSpecialCard && meleeMardroemeCard == null)
            {
                cardOptions.Add(new CardOption(agileMardroemeCard, score, "Mardroeme to transform morph cards on melee row"));
            }
            else if (!isMardroemeActiveOnRanged && shouldMardroemeRangedRow && !hasHornSpecialCard)
            {
                cardOptions.Add(new CardOption(agileMardroemeCard, score, "Mardroeme to transform morph cards on ranged row"));
            }
        }

        // Need to decide which row to add the special mardroeme card
        if (specialMardroemeCard != null)
        {
            if (!isMardroemeActiveOnMelee && shouldMardroemeMeleeRow && isMeleeSpecialEmpty)
            {
                cardOptions.Add(new CardOption(specialMardroemeCard, score - 1, "Mardroeme to transform morph cards on melee row", null, RowZone.OpponentMeleeSpecial));
            }
            else if (!isMardroemeActiveOnRanged && shouldMardroemeRangedRow && isRangedSpecialEmpty)
            {
                cardOptions.Add(new CardOption(specialMardroemeCard, score - 1, "Mardroeme to transform morph cards on ranged row", null, RowZone.OpponentRangedSpecial));
            }
            else if (!isMardroemeActiveOnSiege && shouldMardroemeSiegeRow && isSiegeSpecialEmpty)
            {
                cardOptions.Add(new CardOption(specialMardroemeCard, score - 1, "Mardroeme to transform morph cards on siege row", null, RowZone.OpponentSiegeSpecial));
            }
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
            return npcGraveyard.OrderByDescending(c => c.strength).FirstOrDefault();

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
    /// Gets all cards in zone without an ability.
    /// </summary>
    /// <returns></returns>
    private List<CardData> GetCardsWithoutAbility(List<CardData> zone)
    {
        return zone.Where(c => string.IsNullOrEmpty(c.ability)).ToList();
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

    /// <summary>
    /// Returns true if any weather effect is active on the specified row.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    private bool IsWeatherActiveOnRow(string range)
    {
        // No weather cards active
        if (state.weatherCards == null || state.weatherCards.Count == 0)
            return false;

        bool frostActive = state.weatherCards.Any(c => c.ability == CardDefs.Ability.Frost);
        bool fogActive = state.weatherCards.Any(c => c.ability == CardDefs.Ability.Fog);
        bool rainActive = state.weatherCards.Any(c => c.ability == CardDefs.Ability.Rain);
        bool stormActive = state.weatherCards.Any(c => c.ability == CardDefs.Ability.Storm);
        bool natureActive = state.weatherCards.Any(c => c.ability == CardDefs.Ability.Nature);
        bool whiteFrostActive = state.weatherCards.Any(c => c.ability == CardDefs.Ability.WhiteFrost);

        // Check if weather effect is active on the specified row
        switch (range)
        {
            case CardDefs.Range.Melee:
                return frostActive || natureActive || whiteFrostActive;
            case CardDefs.Range.Ranged:
                return fogActive || stormActive || whiteFrostActive;
            case CardDefs.Range.Siege:
                return rainActive || stormActive || natureActive;
            default:
                return false;
        }
    }

    /// <summary>
    /// Calculates the total strength of standard cards on a row with horn effect.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private int CalculateHornRowStrength(List<CardData> row)
    {
        int strength = 0;
        foreach (var card in row)
        {
            // Skip boosting non-standard cards and horn cards
            if (card.type != CardDefs.Type.Standard || card.ability == CardDefs.Ability.Horn)
                continue;

            strength += card.strength * 2;
        }
        return strength;
    }
}

/// <summary>
/// Card option with score and reason for AI decision making.
/// </summary>
class CardOption
{
    public CardData card;
    public CardData targetCard; // for decoy
    public RowZone targetRow; // for horn and mardroeme
    public int score;
    public string reason;

    public CardOption(CardData card, int score, string reason, CardData targetCard = null, RowZone targetRow = RowZone.None)
    {
        this.card = card;
        this.score = score;
        this.reason = reason;
        this.targetCard = targetCard;
        this.targetRow = targetRow;
    }
}