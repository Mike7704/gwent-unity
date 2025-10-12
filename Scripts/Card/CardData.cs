using System;
using System.Collections.Generic;

[Serializable]
public class CardTarget
{
    public int id;
    public string name;
}

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
    public List<CardTarget> target;
    public string imagePath;  // path to sprite in Resources
    public string videoPath;
    public bool unlocked;
    public bool challengeRewardCard;
}

[Serializable]
public class CardCollection
{
    public string faction;
    public List<CardData> leaders;
    public List<CardData> cards;
}
