using System.Collections.Generic;
using UnityEngine;

public class CardPool : MonoBehaviour
{
    public static CardPool Instance;

    [Header("References")]
    public GameObject cardPrefab;

    [Tooltip("Store inactive cards")]
    public Transform hiddenParent;

    private List<CardUI> pool = new List<CardUI>();

    void Awake()
    {
        if (Instance != null)
        { 
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup hidden parent to hold inactive cards
        if (hiddenParent == null)
        {
            GameObject hidden = new GameObject("CardPool_Hidden");
            hidden.transform.SetParent(transform);
            hiddenParent = hidden.transform;
        }
    }

    /// <summary>
    /// Preload all cards from the database into the pool for UI reuse.
    /// </summary>
    public void PreloadAllCards(List<CardData> allCards)
    {
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogWarning("CardPool: No cards to preload.");
            return;
        }

        foreach (var data in allCards)
        {
            GameObject cardGO = Instantiate(cardPrefab, hiddenParent);
            CardUI cardUI = cardGO.GetComponent<CardUI>();

            if (cardUI == null)
            {
                Debug.LogError("CardPrefab does not have a CardUI component.");
                Destroy(cardGO);
                continue;
            }

            cardUI.Setup(data);       // Pre-setup to cache sprites
            cardGO.SetActive(false);  // Start hidden
            pool.Add(cardUI);
        }

        Debug.Log($"CardPool: Preloaded {pool.Count} cards.");
    }

    /// <summary>
    /// Get a card from the pool, activating it and setting its parent.
    /// </summary>
    public CardUI GetCard(Transform parent = null)
    {
        foreach (var card in pool)
        {
            if (!card.gameObject.activeSelf)
            {
                card.transform.SetParent(parent, false);
                card.gameObject.SetActive(true);
                return card;
            }
        }

        Debug.LogWarning("CardPool exhausted! Consider increasing pool size.");
        return null;
    }

    /// <summary>
    /// Return a card to the pool to be reused.
    /// </summary>
    public void ReturnCard(CardUI card)
    {
        if (card == null) return;
        card.gameObject.SetActive(false);
        card.transform.SetParent(hiddenParent, false);
    }

    /// <summary>
    /// Hide all cards in the pool.
    /// </summary>
    public void HideAll()
    {
        foreach (var card in pool)
        {
            card.gameObject.SetActive(false);
            card.transform.SetParent(hiddenParent, false);
        }
    }
}
