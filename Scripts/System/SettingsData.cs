using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game settings data keys, defaults, and descriptions.
/// </summary>
public static class SettingsData
{
    // Audio
    public const string MuteAudioKey = "MuteAudio";
    public const string MusicVolumeKey = "MusicVolume";
    public const string SFXVolumeKey = "SFXVolume";
    public const string VoiceVolumeKey = "VoiceVolume";
    public const string VoicePercentageChanceKey = "VoicePercentageChance";

    // Video
    public const string FullscreenKey = "Fullscreen";
    public const string VSyncKey = "VSync";
    public const string TargetFrameRateKey = "TargetFrameRate";

    // Gameplay
    public const string InitialHandSizeKey = "InitialHandSize";
    public const string RandomisePlayerDeckKey = "RandomisePlayerDeck";
    public const string RandomiseDeckSizeKey = "RandomiseDeckSize";
    public const string SpyDrawAmountKey = "SpyDrawAmount";
    public const string LeaderCardEnabledKey = "LeaderCardEnabled";
    public const string FactionAbilityEnabledKey = "FactionAbilityEnabled";

    // Defaults
    public const int DefaultMuteAudio = 0;
    public const float DefaultMusicVolume = 0.5f;
    public const float DefaultSFXVolume = 1f;
    public const float DefaultVoiceVolume = 1f;
    public const int DefaultVoicePercentageChance = 100;

    public const int DefaultFullscreen = 1;
    public const int DefaultVSync = 1;
    public const int DefaultTargetFrameRate = 60;

    public const int DefaultInitialHandSize = 10;
    public const int DefaultRandomisePlayerDeck = 0;
    public const int DefaultRandomiseDeckSize = 25;
    public const int DefaultSpyDrawAmount = 2;
    public const int DefaultLeaderCardEnabled = 1;
    public const int DefaultFactionAbilityEnabled = 1;

    // Descriptions
    public static readonly Dictionary<string, string> Descriptions = new()
    {
        // Audio
        { MuteAudioKey, "Completely mutes all game audio." },
        { MusicVolumeKey, "Sets the volume for music." },
        { SFXVolumeKey, "Sets the volume for sound effects." },
        { VoiceVolumeKey, "Sets the volume of voice lines." },
        { VoicePercentageChanceKey, "Determines how often voice lines are played by cards." },

        // Video
        { FullscreenKey, "Toggles between fullscreen and windowed mode." },
        { VSyncKey, "Synchronizes frame rate with your monitor to prevent screen tearing." },
        { TargetFrameRateKey, "Limits the maximum frames per second the game will render." },

        // Gameplay
        { InitialHandSizeKey, "Sets the number of cards you start each match with." },
        { RandomisePlayerDeckKey, "Randomly generates a deck each game." },
        { RandomiseDeckSizeKey, "Sets the number of cards generated when using a random deck." },
        { SpyDrawAmountKey, "Controls how many cards are drawn when playing a Spy card." },
        { LeaderCardEnabledKey, "Enables or disables leader card abilities." },
        { FactionAbilityEnabledKey, "Enables or disables faction abilities." },
    };
}
