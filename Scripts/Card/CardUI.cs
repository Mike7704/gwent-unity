using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;

/// <summary>
/// Handles visual representation of a single card in the UI.
/// Uses CardData from the database and loads assets via ResourceSystem.
/// </summary>
public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    public Image background;
    public Image panel;
    public Image banner;
    public Image border;
    public Image strengthIcon;
    public Image rangeIcon;
    public Image abilityIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quoteText;
    public TextMeshProUGUI strengthText;
    public Image overlayImage;
    public Image factionBack;

    public CardData cardData;

    private bool isSpecialCard = false; // True for Special and Leader cards
    private bool hasBanner = false;
    public bool isCropped = false; // Whether to show the description panel
    public bool isInteractable = true;
    public bool isDraggable = false;

    // Events for managers to subscribe
    public event System.Action<CardUI> OnCardClicked;
    public event System.Action<CardUI> OnCardDragged;

    private Vector3 originalPosition;

    /// <summary>
    /// Setup the card UI elements based on the provided CardData.
    /// </summary>
    public void Setup(CardData data, float width, float height, bool cropped)
    {
        cardData = data;

        if (cardData == null)
        {
            Debug.LogWarning("[CardUI] Tried to setup with null data!");
            return;
        }

        isSpecialCard = (cardData.type == CardDefs.Type.Special || cardData.type == CardDefs.Type.Leader);
        hasBanner = !(cardData.faction == CardDefs.Faction.Neutral || cardData.type == CardDefs.Type.Special || cardData.type == CardDefs.Type.Leader);
        this.isCropped = cropped;
        isInteractable = true;
        isDraggable = false;

        SetBackground();
        SetPanel();
        SetBanner();
        SetBorder(isMouseHover: false);

        SetStrengthIcon();
        SetRangeIcon();
        SetAbilityIcon();

        SetNameText();
        SetQuoteText(width, height);
        SetStrengthText();

        ShowCardSelectedOverlay(false);
        SetFactionBack();
    }

    // Set card background image
    private void SetBackground()
    {
        Sprite art = ResourceSystem.Instance.LoadSprite(cardData.imagePath);
        if (background != null && art != null)
        {
            background.sprite = art;
        }
        else
        {
            Debug.LogWarning("[CardUI] Couldn't draw background image to [" + cardData.name + "]");
        }
    }

    // Set panel image
    private void SetPanel()
    {
        if (panel == null) return;

        if (isCropped)
        {
            panel.enabled = false;
            return;
        }
        panel.enabled = true;

        if (hasBanner)
        {
            if (cardData.type == CardDefs.Type.Hero)
            {
                panel.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_hero_description_banner");
            }
            else
            {
                panel.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_description_banner");
            }
        }
        else
        {
            if (cardData.type == CardDefs.Type.Hero)
            {
                panel.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_hero_description");
            }
            else
            {
                panel.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_description");
            }
        }
    }

    // Set banner image based on faction
    private void SetBanner()
    {
        if (banner == null || !hasBanner)
        {
            banner.enabled = false; // No banner for neutral/special/leader
            return;
        }

        banner.enabled = true;

        switch (cardData.faction)
            {
                case CardDefs.Faction.NorthernRealms:
                    banner.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/banner_northern_realms"); break;
                case CardDefs.Faction.Nilfgaard:
                    banner.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/banner_nilfgaard"); break;
                case CardDefs.Faction.Scoiatael:
                    banner.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/banner_scoiatael"); break;
                case CardDefs.Faction.Monsters:
                    banner.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/banner_monsters"); break;
                case CardDefs.Faction.Skellige:
                    banner.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/banner_skellige"); break;
                default:
                    Debug.LogWarning("[CardUI] Couldn't draw faction banner to [" + cardData.name + "]");
                    banner.enabled = false;
                    break;
            }
    }

    // Set border image based on type and faction
    private void SetBorder(bool isMouseHover)
    {
        if (border == null) return;

        if (isMouseHover)
        {
            border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_glow" + (isCropped ? "_cropped" : ""));
        }
        else if (cardData.type == CardDefs.Type.Hero)
        {
            border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_hero" + (isCropped ? "_cropped" : ""));
        }
        else if (cardData.type == CardDefs.Type.Leader || cardData.type == CardDefs.Type.Special)
        {
            border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_special" + (isCropped ? "_cropped" : ""));
        }
        else
        {
            switch (cardData.faction)
            {
                case CardDefs.Faction.NorthernRealms:
                    border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_northern_realms" + (isCropped ? "_cropped" : "")); break;
                case CardDefs.Faction.Nilfgaard:
                    border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_nilfgaard" + (isCropped ? "_cropped" : "")); break;
                case CardDefs.Faction.Scoiatael:
                    border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_scoiatael" + (isCropped ? "_cropped" : "")); break;
                case CardDefs.Faction.Monsters:
                    border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_monsters" + (isCropped ? "_cropped" : "")); break;
                case CardDefs.Faction.Skellige:
                    border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_skellige" + (isCropped ? "_cropped" : "")); break;
                default:
                    border.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/border_neutral" + (isCropped ? "_cropped" : "")); break;
            }
        }
    }

    // Set strength icon based on type and ability
    private void SetStrengthIcon()
    {
        if (strengthIcon == null) return;

        if (cardData.type == CardDefs.Type.Standard)
        {
            strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/power_normal");
        }
        else if (cardData.type == CardDefs.Type.Hero)
        {
            strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/power_hero");
        }
        else // Special/Leader
        {
            switch (cardData.ability)
            {
                case CardDefs.Ability.Clear:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_clear"); break;
                case CardDefs.Ability.Decoy:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_decoy"); break;
                case CardDefs.Ability.Fog:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_fog"); break;
                case CardDefs.Ability.Frost:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_frost"); break;
                case CardDefs.Ability.Horn:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_horn2"); break;
                case CardDefs.Ability.Mardroeme:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_mardroeme2"); break;
                case CardDefs.Ability.Rain:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_rain"); break;
                case CardDefs.Ability.Scorch:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_scorch"); break;
                case CardDefs.Ability.ScorchRow:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_scorch_row2"); break;
                case CardDefs.Ability.DrawEnemyDiscard:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_spy2"); break;
                case CardDefs.Ability.Storm:
                case CardDefs.Ability.Nature:
                case CardDefs.Ability.WhiteFrost:
                    strengthIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_storm"); break;
                default:
                    Debug.LogWarning("[CardUI] Couldn't draw strength icon to [" + cardData.name + "]");
                    break;
            }
        }
    }

    // Set range icon based on range
    private void SetRangeIcon()
    {
        if (rangeIcon == null || isSpecialCard)
        { 
            rangeIcon.enabled = false; // No range icon for special/leader cards
            return;
        }

        rangeIcon.enabled = true;

        switch (cardData.range)
        {
            case CardDefs.Range.Melee:
                rangeIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_row_close"); break;
            case CardDefs.Range.Agile:
                rangeIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_row_agile"); break;
            case CardDefs.Range.Ranged:
                rangeIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_row_ranged"); break;
            case CardDefs.Range.Siege:
                rangeIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_row_siege"); break;
            default:
                Debug.LogWarning("[CardUI] Couldn't draw range icon to [" + cardData.name + "]");
                rangeIcon.enabled = false;
                break;
        }
    }

    // Set ability icon based on ability
    private void SetAbilityIcon()
    {
        if (abilityIcon == null || isSpecialCard || cardData.ability == null)
        {
            abilityIcon.enabled = false; // No ability icon for special/leader cards
            return;
        }

        abilityIcon.enabled = true;

        switch (cardData.ability)
        {
            case CardDefs.Ability.Avenger:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_avenger"); break;
            case CardDefs.Ability.Morph:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_morph"); break;
            case CardDefs.Ability.Bond:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_bond"); break;
            case CardDefs.Ability.Horn:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_horn"); break;
            case CardDefs.Ability.Mardroeme:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_mardroeme"); break;
            case CardDefs.Ability.Medic:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_medic"); break;
            case CardDefs.Ability.Morale:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_morale"); break;
            case CardDefs.Ability.Muster:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_muster"); break;
            case CardDefs.Ability.MusterPlus:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_muster_plus"); break;
            case CardDefs.Ability.Scorch:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_scorch2"); break;
            case CardDefs.Ability.ScorchRow:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_scorch_row"); break;
            case CardDefs.Ability.Spy:
                abilityIcon.sprite = ResourceSystem.Instance.LoadSprite("Cards/components/card_ability_spy"); break;
            default:
                Debug.LogWarning("[CardUI] Couldn't draw ability icon to [" + cardData.name + "]");
                abilityIcon.enabled = false;
                break;
        }
    }

    // Set card name text
    private void SetNameText()
    {
        if (nameText == null) return;

        if (isCropped)
        {
            nameText.enabled = false;
            return;
        }
        nameText.enabled = true;

        nameText.text = cardData.name;

        // Shift text right to avoid overlapping banner
        RectTransform rect = nameText.GetComponent<RectTransform>();
        rect.anchorMin = (hasBanner ? new Vector2(0.22f, 0.155f) : new Vector2(0.05f, 0.155f));
    }

    // Set card quote text
    private void SetQuoteText(float cardWidth, float cardHeight)
    {
        if (quoteText == null) return;

        if (isCropped)
        {
            quoteText.enabled = false;
            return;
        }
        quoteText.enabled = true;

        string nickname = "";
        string quote = cardData.quote;
        if (quote.Contains("\n"))
        { 
            // Split quote into nickname and quote
            string[] parts = quote.Split(new[] { '\n' }, 2);
            nickname = $"--- {parts[0]} ---\n";
            quote = $"\"{parts[1]}\"";
        }
        else
            quote = $"\"{quote}\"";

        quoteText.text = $"{nickname}<i>{quote}</i>";

        // Shift text right to avoid overlapping banner
        RectTransform rect = quoteText.GetComponent<RectTransform>();
        rect.anchorMin = (hasBanner ? new Vector2(0.22f, 0f) : new Vector2(0.05f, 0f));

        // Dynamically adjust font size based on the text component's current size
        // Font scaling variables
        float minFontSize = 1f;
        float maxFontSize = 30f;
        // Reference values (370x575 card = 20 font)
        float referenceWidth = 370f;
        float referenceHeight = 575f;
        float referenceFontSize = 20f;

        // Font size calculation
        float scaleFactor = referenceFontSize / Mathf.Min(referenceWidth, referenceHeight);
        float targetFontSize = Mathf.Min(cardWidth, cardHeight) * scaleFactor;
        targetFontSize = Mathf.Clamp(targetFontSize, minFontSize, maxFontSize);

        // Apply font settings
        quoteText.fontSize = Mathf.RoundToInt(targetFontSize);
        quoteText.lineSpacing = targetFontSize * -1;
        quoteText.paragraphSpacing = targetFontSize * -1;
    }

    // Set card strength text
    private void SetStrengthText()
    {
        if (strengthText == null) return;

        strengthText.text = (isSpecialCard ? "" : cardData.strength.ToString());
        strengthText.color = (cardData.type == CardDefs.Type.Hero ? Color.white : Color.black);
    }

    // Set overlay image when selected in deck
    public void ShowCardSelectedOverlay(bool show)
    {
        if (overlayImage == null) return;

        overlayImage.gameObject.SetActive(show);
    }

    // Set card back based on faction
    private void SetFactionBack()
    {
        if (factionBack == null) return;
        /*
        if (cardData.unlocked)
        {
            factionBack.enabled = false;
            return;
        }
        */
        factionBack.enabled = false;

        switch (cardData.faction)
        {
            case CardDefs.Faction.NorthernRealms:
                factionBack.sprite = ResourceSystem.Instance.LoadSprite("Faction/Northern_Realms_Card_Back"); break;
            case CardDefs.Faction.Nilfgaard:
                factionBack.sprite = ResourceSystem.Instance.LoadSprite("Faction/Nilfgaard_Card_Back"); break;
            case CardDefs.Faction.Scoiatael:
                factionBack.sprite = ResourceSystem.Instance.LoadSprite("Faction/Scoiatael_Card_Back"); break;
            case CardDefs.Faction.Monsters:
                factionBack.sprite = ResourceSystem.Instance.LoadSprite("Faction/Monsters_Card_Back"); break;
            case CardDefs.Faction.Skellige:
                factionBack.sprite = ResourceSystem.Instance.LoadSprite("Faction/Skellige_Card_Back"); break;
            default:
                if (isSpecialCard)
                {
                    factionBack.sprite = ResourceSystem.Instance.LoadSprite("Faction/Special_Card_Back");
                }
                else
                {
                    factionBack.sprite = ResourceSystem.Instance.LoadSprite("Faction/Neutral_Card_Back");
                }
                break;
        }
    }

    // -------------------------
    // Interaction Handlers
    // -------------------------

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }

    public void SetDraggable(bool value)
    {
        isDraggable = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardData == null || !isInteractable) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnCardClicked?.Invoke(this);
        }

        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            InspectCardWindow.Instance.Show(cardData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardData == null || !isInteractable) return;

        SetBorder(isMouseHover: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (cardData == null || !isInteractable) return;

        SetBorder(isMouseHover: false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cardData == null || !isInteractable || !isDraggable) return;

        originalPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (cardData == null || !isInteractable || !isDraggable) return;

        // Smoothly move card with cursor
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
        transform.parent as RectTransform,
        eventData.position,
        eventData.pressEventCamera,
        out Vector2 localPoint
    );

        transform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (cardData == null || !isInteractable || !isDraggable) return;

        OnCardDragged?.Invoke(this);
        // Reset position if not played
        transform.position = originalPosition;
    }
    
}
