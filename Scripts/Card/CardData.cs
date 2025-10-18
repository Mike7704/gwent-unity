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
