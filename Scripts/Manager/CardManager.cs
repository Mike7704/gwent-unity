using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles spawning, displaying, and updating card UI elements in menus or decks.
/// </summary>
public class CardManager : Singleton<CardManager>
{
    [Header("Card Prefab & Parent")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject cardCroppedPrefab;
    [SerializeField] private Transform cardParent;

    [Header("Runtime Data")]
    public List<CardUI> activeCards = new List<CardUI>();
    // Internal pool for performance
    private readonly Queue<CardUI> cardPool = new Queue<CardUI>();
    private readonly Queue<CardUI> croppedCardPool = new Queue<CardUI>();

    // -----------------------------
    // Card Creation
    // -----------------------------

    /// <summary>
    /// Creates or reuses a CardUI and initialises it with the given data.
    /// </summary>
    /// <param name="data">Card data to display.</param>
    /// <param name="parent">Parent transform for the card (optional).</param>
    /// <returns>Initialised CardUI instance.</returns>
    public CardUI CreateCard(CardData data, bool cropped, Transform parent = null)
    {
        if (cardPrefab == null || cardCroppedPrefab == null)
        {
            Debug.LogError("[CardManager] Missing cardPrefab reference!");
            return null;
        }

        if (parent == null)
            parent = cardParent;

        // Get a pooled or new card
        CardUI cardUI = GetCardFromPool(cropped);
        if (cardUI == null)
            return null;

        // Make sure it's not already in active list
        if (!activeCards.Contains(cardUI))
            activeCards.Add(cardUI);

        cardUI.transform.SetParent(parent, false);
        cardUI.gameObject.SetActive(true);

        // Get card width and height
        float cardWidth = 0f;
        float cardHeight = 0f;
        GridLayoutGroup grid = parent.GetComponent<GridLayoutGroup>();
        CardRowLayout rowLayout = parent.GetComponent<CardRowLayout>();

        if (grid != null)
        {
            cardWidth = grid.cellSize.x;
            cardHeight = grid.cellSize.y;
        }
        else if (rowLayout != null)
        {
            cardWidth = rowLayout.cardWidth;
            cardHeight = rowLayout.cardHeight;
        }
        else
        {
            // Fallback to the prefab RectTransform size
            RectTransform rect = cardPrefab.GetComponent<RectTransform>();
            cardWidth = rect.rect.width;
            cardHeight = rect.rect.height;
        }

        cardUI.Setup(data, cardWidth, cardHeight, cropped);

        return cardUI;
    }

    /// <summary>
    /// Returns a Card back to the pool for reuse.
    /// </summary>
    public void ReturnCard(CardUI card)
    {
        if (card == null) return;

        // Remove from active cards first
        activeCards.Remove(card);
        card.gameObject.SetActive(false);

        // Return to pool
        if (cardParent != null)
            card.transform.SetParent(cardParent, false);
        else
            card.transform.SetParent(null);

        if (card.isCropped)
            croppedCardPool.Enqueue(card);
        else
            cardPool.Enqueue(card);
    }

    // -----------------------------
    // Pool Management
    // -----------------------------

    /// <summary>
    /// Retrieves a card from the pool or instantiates a new one.
    /// </summary>
    private CardUI GetCardFromPool(bool cropped)
    {
        Queue<CardUI> pool = cropped ? croppedCardPool : cardPool;

        // Dequeue until we find a valid, alive CardUI or run out of pool entries
        while (pool.Count > 0)
        {
            var candidate = pool.Dequeue();
            if (candidate == null || candidate.gameObject == null) continue;
            if (!candidate.gameObject.scene.isLoaded) continue;
            return candidate;
        }

        // No valid pooled item — instantiate a new one
        GameObject cardObj = Instantiate(cropped ? cardCroppedPrefab : cardPrefab);
        CardUI cardUI = cardObj.GetComponent<CardUI>();
        if (cardUI == null)
        {
            Debug.LogError("[CardManager] Card prefab missing CardUI component!");
            Destroy(cardObj);
            return null;
        }
        return cardUI;
    }

    /// <summary>
    /// Hides and returns all active cards to the pool.
    /// </summary>
    public void ClearCards()
    {
        foreach (var card in activeCards)
        {
            if (card == null) continue;

            card.gameObject.SetActive(false);
            if (cardParent != null)
                card.transform.SetParent(cardParent, false);
            else
                card.transform.SetParent(null);

            if (card.isCropped)
                croppedCardPool.Enqueue(card);
            else
                cardPool.Enqueue(card);
        }

        activeCards.Clear();
    }

    /// <summary>
    /// Permanently destroys all active and pooled cards.
    /// </summary>
    public void DestroyAllCards()
    {
        foreach (var card in activeCards)
            if (card != null) Destroy(card.gameObject);

        foreach (var card in cardPool)
            if (card != null) Destroy(card.gameObject);

        foreach (var card in croppedCardPool)
            if (card != null) Destroy(card.gameObject);

        activeCards.Clear();
        cardPool.Clear();
        croppedCardPool.Clear();
    }

    /// <summary>
    /// Remove destroyed/null entries from the pool (and active list).
    /// Call this after scene load or before creating many cards.
    /// </summary>
    public void PurgeDestroyedCards()
    {
        // Clean normal pool
        var normalTmp = new Queue<CardUI>();
        while (cardPool.Count > 0)
        {
            var c = cardPool.Dequeue();
            if (c != null && c.gameObject != null && c.gameObject.scene.isLoaded)
                normalTmp.Enqueue(c);
        }
        while (normalTmp.Count > 0)
            cardPool.Enqueue(normalTmp.Dequeue());

        // Clean cropped pool
        var croppedTmp = new Queue<CardUI>();
        while (croppedCardPool.Count > 0)
        {
            var c = croppedCardPool.Dequeue();
            if (c != null && c.gameObject != null && c.gameObject.scene.isLoaded)
                croppedTmp.Enqueue(c);
        }
        while (croppedTmp.Count > 0)
            croppedCardPool.Enqueue(croppedTmp.Dequeue());

        // Clean active cards
        activeCards.RemoveAll(c => c == null || c.gameObject == null);
    }

    /// <summary>
    /// Get the full ability name
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public string GetAbilityOfficalName(CardData cardData)
    {
        switch (cardData.ability)
        {
            case CardDefs.Ability.Clear: return CardDefs.AbilityOfficalName.Clear;
            case CardDefs.Ability.Frost: return CardDefs.AbilityOfficalName.Frost;
            case CardDefs.Ability.Fog: return CardDefs.AbilityOfficalName.Fog;
            case CardDefs.Ability.Rain: return CardDefs.AbilityOfficalName.Rain;
            case CardDefs.Ability.Storm: return cardData.name;
            case CardDefs.Ability.Nature: return cardData.name;
            case CardDefs.Ability.WhiteFrost: return cardData.name;
            case CardDefs.Ability.Avenger: return CardDefs.AbilityOfficalName.Avenger;
            case CardDefs.Ability.Bond: return CardDefs.AbilityOfficalName.Bond;
            case CardDefs.Ability.Decoy: return CardDefs.AbilityOfficalName.Decoy;
            case CardDefs.Ability.DrawEnemyDiscard: return CardDefs.AbilityOfficalName.DrawEnemyDiscard;
            case CardDefs.Ability.Horn: return CardDefs.AbilityOfficalName.Horn;
            case CardDefs.Ability.Mardroeme: return CardDefs.AbilityOfficalName.Mardroeme;
            case CardDefs.Ability.Medic: return CardDefs.AbilityOfficalName.Medic;
            case CardDefs.Ability.Morale: return CardDefs.AbilityOfficalName.Morale;
            case CardDefs.Ability.Morph: return CardDefs.AbilityOfficalName.Morph;
            case CardDefs.Ability.Muster: return CardDefs.AbilityOfficalName.Muster;
            case CardDefs.Ability.MusterPlus: return CardDefs.AbilityOfficalName.MusterPlus;
            case CardDefs.Ability.Scorch: return CardDefs.AbilityOfficalName.Scorch;
            case CardDefs.Ability.ScorchRow: return $"{CardDefs.AbilityOfficalName.ScorchRow}: {TextUtils.CapFirstLetter(cardData.range)}";
            case CardDefs.Ability.Spy: return CardDefs.AbilityOfficalName.Spy;
            default: return "";
        }
    }

    /// <summary>
    /// Get the ability description
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public string GetAbilityDescription(string ability)
    {
        switch (ability)
        {
            case CardDefs.Ability.Clear: return CardDefs.AbilityDescription.Clear;
            case CardDefs.Ability.Frost: return CardDefs.AbilityDescription.Frost;
            case CardDefs.Ability.Fog: return CardDefs.AbilityDescription.Fog;
            case CardDefs.Ability.Rain: return CardDefs.AbilityDescription.Rain;
            case CardDefs.Ability.Storm: return CardDefs.AbilityDescription.Storm;
            case CardDefs.Ability.Nature: return CardDefs.AbilityDescription.Nature;
            case CardDefs.Ability.WhiteFrost: return CardDefs.AbilityDescription.WhiteFrost;
            case CardDefs.Ability.Avenger: return CardDefs.AbilityDescription.Avenger;
            case CardDefs.Ability.Bond: return CardDefs.AbilityDescription.Bond;
            case CardDefs.Ability.Decoy: return CardDefs.AbilityDescription.Decoy;
            case CardDefs.Ability.DrawEnemyDiscard: return CardDefs.AbilityDescription.DrawEnemyDiscard;
            case CardDefs.Ability.Horn: return CardDefs.AbilityDescription.Horn;
            case CardDefs.Ability.Mardroeme: return CardDefs.AbilityDescription.Mardroeme;
            case CardDefs.Ability.Medic: return CardDefs.AbilityDescription.Medic;
            case CardDefs.Ability.Morale: return CardDefs.AbilityDescription.Morale;
            case CardDefs.Ability.Morph: return CardDefs.AbilityDescription.Morph;
            case CardDefs.Ability.Muster: return CardDefs.AbilityDescription.Muster;
            case CardDefs.Ability.MusterPlus: return CardDefs.AbilityDescription.MusterPlus;
            case CardDefs.Ability.Scorch: return CardDefs.AbilityDescription.Scorch;
            case CardDefs.Ability.ScorchRow: return CardDefs.AbilityDescription.ScorchRow;
            case CardDefs.Ability.Spy: return CardDefs.AbilityDescription.Spy;
            default: return "";
        }
    }
}
