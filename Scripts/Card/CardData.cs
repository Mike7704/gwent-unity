using System;
using System.Collections.Generic;

/// <summary>
/// Card data loaded from JSON.
/// </summary>
[Serializable]
public class CardData
{
    public int id;
    public string faction;
    public string name;
    public string quote;
    public int strength;
    public string range;      // "melee", "agile" "ranged", "siege"
    public string type;       // "leader", "hero", "standard"
    public string ability;
    public List<CardTarget> target = new List<CardTarget>();
    public string imagePath;  // path to sprite in Resources
    public string videoPath;
    public bool unlocked;
    public bool challengeRewardCard;

    // Runtime defaults
    [NonSerialized] public int defaultStrength;
    [NonSerialized] public string defaultRange;

    /// <summary>
    /// Returns true if the card is a weather card.
    /// </summary>
    /// <returns></returns>
    public bool IsWeatherCard()
    {
        return ability == CardDefs.Ability.Clear ||
               ability == CardDefs.Ability.Frost ||
               ability == CardDefs.Ability.Fog ||
               ability == CardDefs.Ability.Rain ||
               ability == CardDefs.Ability.Storm ||
               ability == CardDefs.Ability.Nature ||
               ability == CardDefs.Ability.WhiteFrost;
    }

    /// <summary>
    /// Clones the card data for creating instances in gameplay.
    /// </summary>
    /// <returns></returns>
    public CardData Clone()
    {
        var clone = new CardData
        {
            id = id,
            faction = faction,
            name = name,
            quote = quote,
            strength = strength,
            range = range,
            type = type,
            ability = ability,
            target = new List<CardTarget>(target.ConvertAll(t => new CardTarget { id = t.id, name = t.name })),
            imagePath = imagePath,
            videoPath = videoPath,
            unlocked = unlocked,
            challengeRewardCard = challengeRewardCard
        };

        clone.defaultStrength = strength;
        clone.defaultRange = range;

        return clone;
    }
}

/// <summary>
/// Optional target reference used by some abilities (morph, avenger etc.)
/// </summary>
[Serializable]
public class CardTarget
{
    public int id;
    public string name;
}

/// <summary>
/// Used to wrap multiple card lists inside a JSON file.
/// </summary>
[Serializable]
public class CardCollection
{
    public string faction;
    public List<CardData> leaders = new List<CardData>();
    public List<CardData> cards = new List<CardData>();
}
