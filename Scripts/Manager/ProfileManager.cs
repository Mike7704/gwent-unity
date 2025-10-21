using UnityEngine;

/// <summary>
/// Manager for player profile data, including stats and collection progress.
/// </summary>
public class ProfileManager : Singleton<ProfileManager>
{
    // Player stats
    public int Wins { get; private set; }
    public int Draws { get; private set; }
    public int Losses { get; private set; }
    public int HighestRoundScore { get; private set; }
    public string Username { get; private set; }
    public int CoinBalance { get; private set; }

    // Deck completion
    public int TotalCardsUnlocked { get; private set; }
    public int SpecialCardsUnlocked { get; private set; }
    public int NeutralCardsUnlocked { get; private set; }
    public int NorthernRealmsCardsUnlocked { get; private set; }
    public int NilfgaardCardsUnlocked { get; private set; }
    public int ScoiataelCardsUnlocked { get; private set; }
    public int MonstersCardsUnlocked { get; private set; }
    public int SkelligeCardsUnlocked { get; private set; }

    /// <summary>
    /// Saves all current profile data to PlayerPrefs and applies them immediately.
    /// </summary>
    public void SaveProfile()
    {
        PlayerPrefs.SetInt(ProfileData.WinsKey, Wins);
        PlayerPrefs.SetInt(ProfileData.DrawsKey, Draws);
        PlayerPrefs.SetInt(ProfileData.LossesKey, Losses);
        PlayerPrefs.SetInt(ProfileData.HighestRoundScoreKey, HighestRoundScore);
        PlayerPrefs.SetString(ProfileData.UsernameKey, Username);
        PlayerPrefs.SetInt(ProfileData.CoinBalanceKey, CoinBalance);

        PlayerPrefs.SetInt(ProfileData.TotalCardsUnlockedKey, TotalCardsUnlocked);
        PlayerPrefs.SetInt(ProfileData.SpecialCardsUnlockedKey, SpecialCardsUnlocked);
        PlayerPrefs.SetInt(ProfileData.NeutralCardsUnlockedKey, NeutralCardsUnlocked);
        PlayerPrefs.SetInt(ProfileData.NorthernRealmsCardsUnlockedKey, NorthernRealmsCardsUnlocked);
        PlayerPrefs.SetInt(ProfileData.NilfgaardCardsUnlockedKey, NilfgaardCardsUnlocked);
        PlayerPrefs.SetInt(ProfileData.ScoiataelCardsUnlockedKey, ScoiataelCardsUnlocked);
        PlayerPrefs.SetInt(ProfileData.MonstersCardsUnlockedKey, MonstersCardsUnlocked);
        PlayerPrefs.SetInt(ProfileData.SkelligeCardsUnlockedKey, SkelligeCardsUnlocked);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads all profile data from PlayerPrefs or defaults if not set.
    /// </summary>
    public void LoadProfile()
    {
        Wins = PlayerPrefs.GetInt(ProfileData.WinsKey, ProfileData.DefaultStatValue);
        Draws = PlayerPrefs.GetInt(ProfileData.DrawsKey, ProfileData.DefaultStatValue);
        Losses = PlayerPrefs.GetInt(ProfileData.LossesKey, ProfileData.DefaultStatValue);
        HighestRoundScore = PlayerPrefs.GetInt(ProfileData.HighestRoundScoreKey, ProfileData.DefaultStatValue);
        Username = PlayerPrefs.GetString(ProfileData.UsernameKey, ProfileData.DefaultUsername);
        CoinBalance = PlayerPrefs.GetInt(ProfileData.CoinBalanceKey, ProfileData.DefaultStatValue);

        TotalCardsUnlocked = PlayerPrefs.GetInt(ProfileData.TotalCardsUnlockedKey, ProfileData.DefaultTotalCardsUnlocked);
        SpecialCardsUnlocked = PlayerPrefs.GetInt(ProfileData.SpecialCardsUnlockedKey, ProfileData.DefaultSpecialCardsUnlocked);
        NeutralCardsUnlocked = PlayerPrefs.GetInt(ProfileData.NeutralCardsUnlockedKey, ProfileData.DefaultNeutralCardsUnlocked);
        NorthernRealmsCardsUnlocked = PlayerPrefs.GetInt(ProfileData.NorthernRealmsCardsUnlockedKey, ProfileData.DefaultFactionCardsUnlocked);
        NilfgaardCardsUnlocked = PlayerPrefs.GetInt(ProfileData.NilfgaardCardsUnlockedKey, ProfileData.DefaultFactionCardsUnlocked);
        ScoiataelCardsUnlocked = PlayerPrefs.GetInt(ProfileData.ScoiataelCardsUnlockedKey, ProfileData.DefaultFactionCardsUnlocked);
        MonstersCardsUnlocked = PlayerPrefs.GetInt(ProfileData.MonstersCardsUnlockedKey, ProfileData.DefaultFactionCardsUnlocked);
        SkelligeCardsUnlocked = PlayerPrefs.GetInt(ProfileData.SkelligeCardsUnlockedKey, ProfileData.DefaultFactionCardsUnlocked);
    }

    /// <summary>
    /// Resets all profile data to their default values, saves, and applies them.
    /// </summary>
    public void ResetProfile()
    {
        Wins = ProfileData.DefaultStatValue;
        Draws = ProfileData.DefaultStatValue;
        Losses = ProfileData.DefaultStatValue;
        HighestRoundScore = ProfileData.DefaultStatValue;
        Username = ProfileData.DefaultUsername;
        CoinBalance = ProfileData.DefaultStatValue;

        TotalCardsUnlocked = ProfileData.DefaultTotalCardsUnlocked;
        SpecialCardsUnlocked = ProfileData.DefaultSpecialCardsUnlocked;
        NeutralCardsUnlocked = ProfileData.DefaultNeutralCardsUnlocked;
        NorthernRealmsCardsUnlocked = ProfileData.DefaultFactionCardsUnlocked;
        NilfgaardCardsUnlocked = ProfileData.DefaultFactionCardsUnlocked;
        ScoiataelCardsUnlocked = ProfileData.DefaultFactionCardsUnlocked;
        MonstersCardsUnlocked = ProfileData.DefaultFactionCardsUnlocked;
        SkelligeCardsUnlocked = ProfileData.DefaultFactionCardsUnlocked;

        DeckManager.Instance.DeleteSavedDeck();

        SaveProfile();

        Debug.Log("[ProfileManager] Profile reset to default values.");
    }

    public void AddWin() { Wins++; SaveProfile(); }
    public void AddDraw() { Draws++; SaveProfile(); }
    public void AddLoss() { Losses++; SaveProfile(); }

    public void UpdateHighestScore(int newScore)
    {
        if (newScore > HighestRoundScore)
        {
            HighestRoundScore = newScore;
            SaveProfile();
        }
    }

    public void AddCoin(int amount)
    {
        CoinBalance += amount;
        SaveProfile();
    }

    public void SetUsername(string username)
    {
        if (username.Length > 15)
        {
            Debug.LogWarning("[ProfileManager] Username too long.");
            return;
        }

        Username = username;
        SaveProfile();
    }

    public void UnlockCard(string faction)
    {
        switch (faction)
        {
            case CardDefs.Faction.Special: SpecialCardsUnlocked++; break;
            case CardDefs.Faction.Neutral: NeutralCardsUnlocked++; break;
            case CardDefs.Faction.NorthernRealms: NorthernRealmsCardsUnlocked++; break;
            case CardDefs.Faction.Nilfgaard: NilfgaardCardsUnlocked++; break;
            case CardDefs.Faction.Scoiatael: ScoiataelCardsUnlocked++; break;
            case CardDefs.Faction.Monsters: MonstersCardsUnlocked++; break;
            case CardDefs.Faction.Skellige: SkelligeCardsUnlocked++; break;
        }

        TotalCardsUnlocked++;
        SaveProfile();
    }
}
