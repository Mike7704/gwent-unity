using System.Collections.Generic;
using UnityEngine;

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

        cardUI.Setup(data, cropped);

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
        // Dequeue until we find a valid, alive CardUI or run out of pool entries
        while (cardPool.Count > 0)
        {
            var candidate = cardPool.Dequeue();
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
            card.gameObject.SetActive(false);
            if (cardParent != null)
                card.transform.SetParent(cardParent, false);
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
            Destroy(card.gameObject);

        foreach (var card in cardPool)
            Destroy(card.gameObject);

        activeCards.Clear();
        cardPool.Clear();
    }

    /// <summary>
    /// Remove destroyed/null entries from the pool (and active list).
    /// Call this after scene load or before creating many cards.
    /// </summary>
    public void PurgeDestroyedCards()
    {
        var tmp = new Queue<CardUI>();

        while (cardPool.Count > 0)
        {
            var c = cardPool.Dequeue();
            if (c != null && c.gameObject != null && c.gameObject.scene.isLoaded)
                tmp.Enqueue(c);
        }

        while (tmp.Count > 0)
            cardPool.Enqueue(tmp.Dequeue());

        activeCards.RemoveAll(c => c == null || c.gameObject == null);
    }
}
