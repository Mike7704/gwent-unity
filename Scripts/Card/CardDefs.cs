using System;
using System.Collections.Generic;

/// <summary>
///  String constants for card properties.
/// </summary>
public static class CardDefs
{
    public static List<string> GetPlayableFactions()
    {
        return new List<string>
        {
            Faction.NorthernRealms,
            Faction.Nilfgaard,
            Faction.Scoiatael,
            Faction.Monsters,
            Faction.Skellige
        };
    }

    public static class Faction
    {
        public const string Special = "Special";
        public const string Neutral = "Neutral";
        public const string NorthernRealms = "Northern Realms";
        public const string Nilfgaard = "Nilfgaard";
        public const string Scoiatael = "Scoiatael";
        public const string Monsters = "Monsters";
        public const string Skellige = "Skellige";
    }

    public static class Type
    {
        public const string Standard = "standard";
        public const string Hero = "hero";
        public const string Special = "special";
        public const string Leader = "leader";
    }

    public static class Range
    {
        public const string Melee = "melee";
        public const string Agile = "agile";
        public const string Ranged = "ranged";
        public const string Siege = "siege";
    }

    public static class Ability
    {
        public const string None = "none";
        public const string Clear = "clear";
        public const string Frost = "frost";
        public const string Fog = "fog";
        public const string Rain = "rain";
        public const string Storm = "storm";
        public const string Nature = "nature";
        public const string WhiteFrost = "whitefrost";
        public const string Avenger = "avenger";
        public const string Bond = "bond";
        public const string Decoy = "decoy";
        public const string DrawEnemyDiscard = "drawenemydiscard";
        public const string Horn = "horn";
        public const string Mardroeme = "mardroeme";
        public const string Medic = "medic";
        public const string Morale = "morale";
        public const string Morph = "morph";
        public const string Muster = "muster";
        public const string MusterPlus = "musterplus";
        public const string Scorch = "scorch";
        public const string ScorchRow = "scorchrow";
        public const string Spy = "spy";
    }
}