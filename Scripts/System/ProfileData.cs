using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Profile data keys and defaults.
/// </summary>
public static class ProfileData
{
    // Player stats
    public const string WinsKey = "Profile_Wins";
    public const string DrawsKey = "Profile_Draws";
    public const string LossesKey = "Profile_Losses";
    public const string HighestRoundScoreKey = "Profile_HighestRoundScore";
    public const string UsernameKey = "Profile_Username";
    public const string CoinBalanceKey = "Profile_CoinBalance";

    // Deck completion
    public const string TotalCardsUnlockedKey = "Profile_TotalCardsUnlocked";
    public const string SpecialCardsUnlockedKey = "Profile_SpecialCardsUnlocked";
    public const string NeutralCardsUnlockedKey = "Profile_NeutralCardsUnlocked";
    public const string NorthernRealmsCardsUnlockedKey = "Profile_NorthernRealmsCardsUnlocked";
    public const string NilfgaardCardsUnlockedKey = "Profile_NilfgaardCardsUnlocked";
    public const string ScoiataelCardsUnlockedKey = "Profile_ScoiataelCardsUnlocked";
    public const string MonstersCardsUnlockedKey = "Profile_MonstersCardsUnlocked";
    public const string SkelligeCardsUnlockedKey = "Profile_SkelligeCardsUnlocked";

    // Defaults
    public const int DefaultStatValue = 0;
    public const string DefaultUsername = "Player";
    public const int DefaultSpecialCardsUnlocked = 0;
    public const int DefaultNeutralCardsUnlocked = 0;
    public const int DefaultFactionCardsUnlocked = 0;
    public const int DefaultTotalCardsUnlocked = DefaultSpecialCardsUnlocked + DefaultNeutralCardsUnlocked + DefaultFactionCardsUnlocked;
}
