using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler
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

    /// <summary>
    /// Setup the card UI elements based on the provided CardData.
    /// </summary>
    public void Setup(CardData data)
    {
        cardData = data;

        isSpecialCard = (cardData.type == "special" || cardData.type == "leader");
        hasBanner = !(cardData.faction == "Neutral" || cardData.type == "special" || cardData.type == "leader");

        SetBackground();
        SetPanel();
        SetBanner();
        SetBorder();

        SetStrengthIcon();
        SetRangeIcon();
        SetAbilityIcon();

        SetNameText();
        SetQuoteText();
        SetStrengthText();

        ShowCardSelectedOverlay(false);
        SetFactionBack();
    }

    // Set card background image
    private void SetBackground()
    {
        Sprite art = SpriteCache.Load(cardData.imagePath);
        if (background != null && art != null)
        {
            background.sprite = art;
        }
        else
        {
            Debug.LogWarning("Couldn't draw background image to [" + cardData.name + "]");
        }
    }

    // Set panel image
    private void SetPanel()
    {
        if (panel == null) return;

        if (hasBanner)
        {
            if (cardData.type == "hero")
            {
                panel.sprite = SpriteCache.Load("Cards/components/card_hero_description_banner");
            }
            else
            {
                panel.sprite = SpriteCache.Load("Cards/components/card_description_banner");
            }
        }
        else
        {
            if (cardData.type == "hero")
            {
                panel.sprite = SpriteCache.Load("Cards/components/card_hero_description");
            }
            else
            {
                panel.sprite = SpriteCache.Load("Cards/components/card_description");
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
                case "Northern Realms": banner.sprite = SpriteCache.Load("Cards/components/banner_northern_realms"); break;
                case "Nilfgaard": banner.sprite = SpriteCache.Load("Cards/components/banner_nilfgaard"); break;
                case "Scoiatael": banner.sprite = SpriteCache.Load("Cards/components/banner_scoiatael"); break;
                case "Monsters": banner.sprite = SpriteCache.Load("Cards/components/banner_monsters"); break;
                case "Skellige": banner.sprite = SpriteCache.Load("Cards/components/banner_skellige"); break;
                default:
                    Debug.LogWarning("Couldn't draw faction banner to [" + cardData.name + "]");
                    banner.enabled = false;
                    break;
            }
    }

    // Set border image based on type and faction
    private void SetBorder()
    {
        if (border == null) return;

        if (cardData.type == "hero")
        {
            border.sprite = SpriteCache.Load("Cards/components/border_hero");
        }
        else if (cardData.type == "leader" || cardData.type == "special")
        {
            border.sprite = SpriteCache.Load("Cards/components/border_special");
        }
        else
        {
            switch (cardData.faction)
            {
                case "Northern Realms": border.sprite = SpriteCache.Load("Cards/components/border_northern_realms"); break;
                case "Nilfgaard": border.sprite = SpriteCache.Load("Cards/components/border_nilfgaard"); break;
                case "Scoiatael": border.sprite = SpriteCache.Load("Cards/components/border_scoiatael"); break;
                case "Monsters": border.sprite = SpriteCache.Load("Cards/components/border_monsters"); break;
                case "Skellige": border.sprite = SpriteCache.Load("Cards/components/border_skellige"); break;
                default: border.sprite = SpriteCache.Load("Cards/components/border_neutral"); break;
            }
        }
    }

    // Set strength icon based on type and ability
    private void SetStrengthIcon()
    {
        if (strengthIcon == null) return;

        if (cardData.type == "standard")
        {
            strengthIcon.sprite = SpriteCache.Load("Cards/components/power_normal");
        }
        else if (cardData.type == "hero")
        {
            strengthIcon.sprite = SpriteCache.Load("Cards/components/power_hero");
        }
        else // Special/Leader
        {
            switch (cardData.ability)
            {
                case "clear": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_clear"); break;
                case "decoy": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_decoy"); break;
                case "fog": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_fog"); break;
                case "frost": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_frost"); break;
                case "horn": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_horn2"); break;
                case "mardroeme": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_mardroeme2"); break;
                case "rain": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_rain"); break;
                case "scorch": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_scorch"); break;
                case "scorchrow": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_scorch_row2"); break;
                case "drawenemydiscard": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_spy2"); break;
                case "storm":
                case "nature":
                case "whitefrost": strengthIcon.sprite = SpriteCache.Load("Cards/components/card_ability_storm"); break;
                default:
                    Debug.LogWarning("Couldn't draw strength icon to [" + cardData.name + "]");
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
            case "melee": rangeIcon.sprite = SpriteCache.Load("Cards/components/card_row_close"); break;
            case "agile": rangeIcon.sprite = SpriteCache.Load("Cards/components/card_row_agile"); break;
            case "ranged": rangeIcon.sprite = SpriteCache.Load("Cards/components/card_row_ranged"); break;
            case "siege": rangeIcon.sprite = SpriteCache.Load("Cards/components/card_row_siege"); break;
            default:
                Debug.LogWarning("Couldn't draw range icon to [" + cardData.name + "]");
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
            case "avenger": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_avenger"); break;
            case "morph": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_morph"); break;
            case "bond": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_bond"); break;
            case "horn": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_horn"); break;
            case "mardroeme": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_mardroeme"); break;
            case "medic": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_medic"); break;
            case "morale": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_morale"); break;
            case "muster": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_muster"); break;
            case "musterplus": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_muster_plus"); break;
            case "scorch": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_scorch2"); break;
            case "scorchrow": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_scorch_row"); break;
            case "spy": abilityIcon.sprite = SpriteCache.Load("Cards/components/card_ability_spy"); break;
            default:
                Debug.LogWarning("Couldn't draw ability icon to [" + cardData.name + "]");
                abilityIcon.enabled = false;
                break;
        }
    }

    // Set card name text
    private void SetNameText()
    {
        if (nameText == null) return;

        nameText.text = cardData.name;

        // Shift text right to avoid overlapping banner
        RectTransform rect = nameText.GetComponent<RectTransform>();
        rect.anchorMin = (hasBanner ? new Vector2(0.22f, 0.155f) : new Vector2(0.05f, 0.155f));
    }

    // Set card quote text
    private void SetQuoteText()
    {
        if (quoteText == null) return;

        quoteText.text = "\"" + cardData.quote + "\"";

        // Shift text right to avoid overlapping banner
        RectTransform rect = quoteText.GetComponent<RectTransform>();
        rect.anchorMin = (hasBanner ? new Vector2(0.22f, 0.11f) : new Vector2(0.05f, 0.11f));
    }

    // Set card strength text
    private void SetStrengthText()
    {
        if (strengthText == null) return;

        strengthText.text = (isSpecialCard ? "" : cardData.strength.ToString());
        strengthText.color = (cardData.type == "hero" ? Color.white : Color.black);
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
            case "Northern Realms": factionBack.sprite = SpriteCache.Load("Faction/Northern_Realms_Card_Back"); break;
            case "Nilfgaard": factionBack.sprite = SpriteCache.Load("Faction/Nilfgaard_Card_Back"); break;
            case "Scoiatael": factionBack.sprite = SpriteCache.Load("Faction/Scoiatael_Card_Back"); break;
            case "Monsters": factionBack.sprite = SpriteCache.Load("Faction/Monsters_Card_Back"); break;
            case "Skellige": factionBack.sprite = SpriteCache.Load("Faction/Skellige_Card_Back"); break;
            default:
                if (isSpecialCard)
                {
                    factionBack.sprite = SpriteCache.Load("Faction/Special_Card_Back");
                }
                else
                {
                    factionBack.sprite = SpriteCache.Load("Faction/Neutral_Card_Back");
                }
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Clicked card: " + cardData.name);
        //Debug.Log("Card Data: " + JsonUtility.ToJson(cardData, true));
        DeckMenu.Instance.OnCardClicked(this);
    }

}
