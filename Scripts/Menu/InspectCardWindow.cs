using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a given card and details about it.
/// </summary>
public class InspectCardWindow : Singleton<InspectCardWindow>
{
    [Header("UI References")]
    public GameObject window;
    public Transform cardContainer;
    public Image panelImage;
    public Image lockedImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI factionText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI abilityText;
    public TextMeshProUGUI abilityDescriptionText;
    public Transform cardTargetsScroll;  // Scroll View content
    public Button closeArea;
    public Button closeButton;

    [Header("Faction Panel Images")]
    public Sprite[] factionSprites;

    void Start()
    {
        closeArea.onClick.AddListener(OnCloseClicked);
        closeButton.onClick.AddListener(OnCloseClicked);

        window.SetActive(false);
    }

    /// <summary>
    /// Show the inspect card window of the selected card.
    /// </summary>
    public void Show(CardData cardData)
    {
        Debug.Log($"[InspectCard] Inspecting [{cardData.name}]");
        
        SetFactionPanelImage(cardData.faction);
        SetCardInfo(cardData);
        DisplayCard(cardData);
        DisplayTargets(cardData);

        window.SetActive(true);
        closeButton.Select(); // Focus on Close button by default
    }

    /// <summary>
    /// Displays information about the selected card
    /// </summary>
    /// <param name="cardData"></param>
    private void SetCardInfo(CardData cardData)
    {
        string type = string.IsNullOrEmpty(cardData.type) ? "N/A" : TextUtils.CapFirstLetter(cardData.type);
        string range = string.IsNullOrEmpty(cardData.range) ? "N/A" : TextUtils.CapFirstLetter(cardData.range);
        string ability = string.IsNullOrEmpty(cardData.ability) ? "N/A" : CardManager.Instance.GetAbilityOfficalName(cardData);
        string abilityDesc = CardManager.Instance.GetAbilityDescription(cardData.ability);

        nameText.text = cardData.name;
        factionText.text = cardData.faction;
        typeText.text = $"<b><color=#660205>Type:</color></b> {type}";
        rangeText.text = $"<b><color=#660205>Range:</color></b> {range}";
        abilityText.text = $"<b><color=#660205>Ability:</color></b> {ability}";
        abilityDescriptionText.text = abilityDesc;
        lockedImage.enabled = !CardDatabase.Instance.GetCardById(cardData.id).unlocked; // Check decks if unlocked
    }

    /// <summary>
    /// Displays the selected card
    /// </summary>
    /// <param name="cardData"></param>
    private void DisplayCard(CardData cardData)
    {
        ClearExistingCards(cardContainer);
        CardUI card = CardManager.Instance.CreateCard(cardData, cropped: false, parent: cardContainer);
        card.SetInteractable(false);
    }

    /// <summary>
    /// Displays all the cards targeted by certain abilities such as muster
    /// </summary>
    /// <param name="cardData"></param>
    private void DisplayTargets(CardData cardData)
    {
        ClearExistingCards(cardTargetsScroll);

        if (cardData.target == null || cardData.target.Count == 0) return;

        foreach (var target in cardData.target)
        {
            CardData targetCard = CardDatabase.Instance.GetCardById(target.id);
            if (targetCard != null)
            {
                CardUI card = CardManager.Instance.CreateCard(targetCard, cropped: false, parent: cardTargetsScroll);
                card.OnCardClicked += OnTargetCardClicked;
            }
            else
                Debug.LogWarning($"[InspectCard] Could not find card with ID {target.id} in database!");
        }
    }

    /// <summary>
    /// Removes previous cards
    /// </summary>
    /// <param name="container"></param>
    private void ClearExistingCards(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            CardUI card = container.GetChild(i).GetComponent<CardUI>();
            if (card != null)
            {
                card.OnCardClicked -= OnTargetCardClicked;  // Unsubscribe
                CardManager.Instance.ReturnCard(card);
            }
        }
    }

    /// <summary>
    /// Display faction header panel
    /// </summary>
    /// <param name="faction"></param>
    private void SetFactionPanelImage(string faction)
    {
        switch (faction)
        {
            case CardDefs.Faction.Neutral:
                panelImage.sprite = factionSprites[0];
                break;
            case CardDefs.Faction.Special:
                panelImage.sprite = factionSprites[1];
                break;
            case CardDefs.Faction.NorthernRealms:
                panelImage.sprite = factionSprites[2];
                break;
            case CardDefs.Faction.Nilfgaard:
                panelImage.sprite = factionSprites[3];
                break;
            case CardDefs.Faction.Scoiatael:
                panelImage.sprite = factionSprites[4];
                break;
            case CardDefs.Faction.Monsters:
                panelImage.sprite = factionSprites[5];
                break;
            case CardDefs.Faction.Skellige:
                panelImage.sprite = factionSprites[6];
                break;
            default:
                panelImage.sprite = factionSprites[0];
                break;
        }
    }
    /// <summary>
    /// Allow user to inspect the target cards
    /// </summary>
    /// <param name="clickedCard"></param>
    private void OnTargetCardClicked(CardUI clickedCard)
    {
        Show(clickedCard.cardData);
    }

    private void OnCloseClicked()
    {
        ClearExistingCards(cardContainer);
        ClearExistingCards(cardTargetsScroll);
        window.SetActive(false);
    }
}
