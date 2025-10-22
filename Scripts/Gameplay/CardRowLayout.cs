using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class CardRowLayout : MonoBehaviour
{
    [Header("Card Layout Settings")]
    public float cardWidth = 96f;
    public float cardHeight = 150f;
    public float spacing = 0f;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        LayoutCards();
    }

    /// <summary>
    /// Repositions and resizes child card RectTransforms to fit within the container,
    /// </summary>
    public void LayoutCards()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        // Collect active card children
        List<RectTransform> cards = new List<RectTransform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                cards.Add(child.GetComponent<RectTransform>());
        }

        if (cards.Count == 0)
            return;

        float containerWidth = rectTransform.rect.width;

        // Ideal total width with normal spacing
        float idealWidth = cards.Count * cardWidth + (cards.Count - 1) * spacing;

        // If too wide, reduce spacing (possibly to overlap)
        float actualSpacing = spacing;
        if (idealWidth > containerWidth)
        {
            float availableWidth = containerWidth - cardWidth;
            actualSpacing = availableWidth / (cards.Count - 1) - cardWidth;

            // Prevent cards from going too far apart or overlapping negatively (max 80% of width)
            actualSpacing = Mathf.Clamp(actualSpacing, -cardWidth * 0.8f, spacing);
        }

        // Compute total width actually used
        float totalUsedWidth = (cards.Count - 1) * (cardWidth + actualSpacing) + cardWidth;

        // Start X so cards are centered
        float startX = -totalUsedWidth / 2f + cardWidth / 2f;

        // Position and size each card
        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform card = cards[i];
            card.sizeDelta = new Vector2(cardWidth, cardHeight);

            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);

            Vector3 pos = new Vector3(startX + i * (cardWidth + actualSpacing), 0f, 0f);
            card.anchoredPosition = pos;
        }
    }
}
