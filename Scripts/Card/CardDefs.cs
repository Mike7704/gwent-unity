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

    public static class AbilityOfficalName
    {
        public const string Clear = "Clear Weather";
        public const string Frost = "Biting Frost";
        public const string Fog = "Impenetrable Fog";
        public const string Rain = "Torrential Rain";
        public const string Storm = "Storm";
        public const string Nature = "Nature";
        public const string WhiteFrost = "White Frost";
        public const string Avenger = "Avenger";
        public const string Bond = "Tight Bond";
        public const string Decoy = "Decoy";
        public const string DrawEnemyDiscard = "Draw Enemy Discard";
        public const string Horn = "Commander's Horn";
        public const string Mardroeme = "Mardroeme";
        public const string Medic = "Medic";
        public const string Morale = "Morale Boost";
        public const string Morph = "Morph";
        public const string Muster = "Muster";
        public const string MusterPlus = "Muster Plus";
        public const string Scorch = "Scorch";
        public const string ScorchRow = "Scorch Row";
        public const string Spy = "Spy";
    }

    public static class AbilityDescription
    {
        public const string Clear = "Removes all weather cards effects from the board.";
        public const string Frost = "Reduces the Strength of all Close Units to 1. Ineffective on Hero cards.";
        public const string Fog = "Reduces the Strength of all Ranged Units to 1. Ineffective on Hero cards.";
        public const string Rain = "Reduces the Strength of all Siege Units to 1. Ineffective on Hero cards.";
        public const string Storm = "Reduces the Strength of all Ranged and Siege Units to 1. Ineffective on Hero cards.";
        public const string Nature = "Reduces the Strength of all Melee and Siege Units to 1. Ineffective on Hero cards.";
        public const string WhiteFrost = "Reduces the Strength of all Melee and Ranged Units to 1. Ineffective on Hero cards.";
        public const string Avenger = "When this card is removed from the board, it summons a new unit card to take its place.";
        public const string Bond = "Doubles the strength of both cards when placed next to a specified unit.";
        public const string Decoy = "Swap with a card on the board to return it to your hand.";
        public const string DrawEnemyDiscard = "Draw a random card from your opponent's discard pile.";
        public const string Horn = "Doubles the strength of all units on a given row, excluding itself (limit one per row).";
        public const string Mardroeme = "Triggers transformations of all Morph cards on a given row.";
        public const string Medic = "Choose one card from your discard pile and play it instantly (no heroes or special cards).";
        public const string Morale = "Adds +1 strength to all cards on its row (excluding itself).";
        public const string Morph = "Transforms into a new card when a Mardroeme card is on its row.";
        public const string Muster = "Finds any specified cards in your deck and plays them instantly.";
        public const string MusterPlus = "Plays the specified cards instantly. These cards aren't required to be in your deck and can only be summoned once.";
        public const string Scorch = "Destroy the highest strengthed card(s) on the board. This includes your cards.";
        public const string ScorchRow = "Destroys your opponent’s strongest card(s) on the corresponding row if the combined strength of units is 10 or more.";
        public const string Spy = "Place on your opponent's side of the board (counts towards their total strength) and draw cards from your deck.";
    }
}