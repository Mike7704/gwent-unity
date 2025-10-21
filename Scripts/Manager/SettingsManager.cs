using System;
using UnityEngine;

/// <summary>
/// Central manager for loading, saving, and applying all game settings.
/// </summary>
public class SettingsManager : Singleton<SettingsManager>
{
    // Audio
    public bool MuteAudio { get; private set; }
    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public float VoiceVolume { get; private set; }
    public int VoicePercentageChance { get; private set; }

    // Video
    public bool Fullscreen { get; private set; }
    public bool VSync { get; private set; }
    public int TargetFrameRate { get; private set; }

    // Gameplay
    public int InitialHandSize { get; private set; }
    public bool RandomisePlayerDeck { get; private set; }
    public int RandomiseDeckSize { get; private set; }
    public int SpyDrawAmount { get; private set; }
    public bool LeaderCardEnabled { get; private set; }
    public bool FactionAbilityEnabled { get; private set; }

    /// <summary>
    /// Saves all current settings to PlayerPrefs and applies them immediately.
    /// </summary>
    public void SaveSettings()
    {
        // Audio
        SetBool(SettingsData.MuteAudioKey, MuteAudio);
        PlayerPrefs.SetFloat(SettingsData.MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetFloat(SettingsData.SFXVolumeKey, SFXVolume);
        PlayerPrefs.SetFloat(SettingsData.VoiceVolumeKey, VoiceVolume);
        PlayerPrefs.SetInt(SettingsData.VoicePercentageChanceKey, VoicePercentageChance);

        // Video
        SetBool(SettingsData.FullscreenKey, Fullscreen);
        SetBool(SettingsData.VSyncKey, VSync);
        PlayerPrefs.SetInt(SettingsData.TargetFrameRateKey, TargetFrameRate);

        // Gameplay
        PlayerPrefs.SetInt(SettingsData.InitialHandSizeKey, InitialHandSize);
        SetBool(SettingsData.RandomisePlayerDeckKey, RandomisePlayerDeck);
        PlayerPrefs.SetInt(SettingsData.RandomiseDeckSizeKey, RandomiseDeckSize);
        PlayerPrefs.SetInt(SettingsData.SpyDrawAmountKey, SpyDrawAmount);
        SetBool(SettingsData.LeaderCardEnabledKey, LeaderCardEnabled);
        SetBool(SettingsData.FactionAbilityEnabledKey, FactionAbilityEnabled);

        PlayerPrefs.Save();
        ApplyAllSettings();
        Debug.Log("[SettingsManager] Settings saved and applied.");
    }

    /// <summary>
    /// Loads all settings from PlayerPrefs or defaults if not set.
    /// </summary>
    public void LoadSettings()
    {
        // Audio
        MuteAudio = GetBool(SettingsData.MuteAudioKey, SettingsData.DefaultMuteAudio);
        MusicVolume = PlayerPrefs.GetFloat(SettingsData.MusicVolumeKey, SettingsData.DefaultMusicVolume);
        SFXVolume = PlayerPrefs.GetFloat(SettingsData.SFXVolumeKey, SettingsData.DefaultSFXVolume);
        VoiceVolume = PlayerPrefs.GetFloat(SettingsData.VoiceVolumeKey, SettingsData.DefaultVoiceVolume);
        VoicePercentageChance = PlayerPrefs.GetInt(SettingsData.VoicePercentageChanceKey, SettingsData.DefaultVoicePercentageChance);

        // Video
        Fullscreen = GetBool(SettingsData.FullscreenKey, SettingsData.DefaultFullscreen);
        VSync = GetBool(SettingsData.VSyncKey, SettingsData.DefaultVSync);
        TargetFrameRate = PlayerPrefs.GetInt(SettingsData.TargetFrameRateKey, SettingsData.DefaultTargetFrameRate);

        // Gameplay
        InitialHandSize = PlayerPrefs.GetInt(SettingsData.InitialHandSizeKey, SettingsData.DefaultInitialHandSize);
        RandomisePlayerDeck = GetBool(SettingsData.RandomisePlayerDeckKey, SettingsData.DefaultRandomisePlayerDeck);
        RandomiseDeckSize = PlayerPrefs.GetInt(SettingsData.RandomiseDeckSizeKey, SettingsData.DefaultRandomiseDeckSize);
        SpyDrawAmount = PlayerPrefs.GetInt(SettingsData.SpyDrawAmountKey, SettingsData.DefaultSpyDrawAmount);
        LeaderCardEnabled = GetBool(SettingsData.LeaderCardEnabledKey, SettingsData.DefaultLeaderCardEnabled);
        FactionAbilityEnabled = GetBool(SettingsData.FactionAbilityEnabledKey, SettingsData.DefaultFactionAbilityEnabled);

        ApplyAllSettings();
    }

    public void ApplyAllSettings()
    {
        ApplyAudioSettings();
        ApplyVideoSettings();
    }

    public void ApplyAudioSettings()
    {
        if (AudioSystem.Instance == null) return;
        AudioSystem.Instance.SetMuteAudio(MuteAudio);
        AudioSystem.Instance.SetMusicVolume(MusicVolume);
        AudioSystem.Instance.SetSFXVolume(SFXVolume);
        AudioSystem.Instance.SetVoiceVolume(VoiceVolume);
        AudioSystem.Instance.SetVoicePercentageChance(VoicePercentageChance);
    }

    public void ApplyVideoSettings()
    {
        Screen.fullScreen = Fullscreen;
        //Screen.SetResolution(1920, 1080, Fullscreen);
        QualitySettings.vSyncCount = (VSync ? 1 : 0);
        Application.targetFrameRate = TargetFrameRate;
    }

    /// <summary>
    /// Sets a specific setting by key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetSetting(string key, object value)
    {
        switch (key)
        {
            // Audio
            case SettingsData.MuteAudioKey:
                MuteAudio = (bool)value;
                ApplyAudioSettings();
                break;
            case SettingsData.MusicVolumeKey:
                MusicVolume = (float)value;
                ApplyAudioSettings();
                break;
            case SettingsData.SFXVolumeKey:
                SFXVolume = (float)value;
                ApplyAudioSettings();
                break;
            case SettingsData.VoiceVolumeKey:
                VoiceVolume = (float)value;
                ApplyAudioSettings();
                break;
            case SettingsData.VoicePercentageChanceKey:
                VoicePercentageChance = (int)value;
                ApplyAudioSettings();
                break;

            // Video
            case SettingsData.FullscreenKey:
                Fullscreen = (bool)value;
                break;
            case SettingsData.VSyncKey:
                VSync = (bool)value;
                break;
            case SettingsData.TargetFrameRateKey:
                TargetFrameRate = (int)value;
                break;

            // Gameplay
            case SettingsData.InitialHandSizeKey:
                InitialHandSize = (int)value;
                break;
            case SettingsData.RandomisePlayerDeckKey:
                RandomisePlayerDeck = (bool)value;
                break;
            case SettingsData.RandomiseDeckSizeKey:
                RandomiseDeckSize = (int)value;
                break;
            case SettingsData.SpyDrawAmountKey:
                SpyDrawAmount = (int)value;
                break;
            case SettingsData.LeaderCardEnabledKey:
                LeaderCardEnabled = (bool)value;
                break;
            case SettingsData.FactionAbilityEnabledKey:
                FactionAbilityEnabled = (bool)value;
                break;

            default:
                Debug.LogWarning($"[SettingsManager] Unknown setting key: {key}");
                break;
        }
    }

    /// <summary>
    /// Gets a specific setting by key.
    /// </summary>
    /// <param name="key"></param>
    public T GetSettingValue<T>(string key)
    {
        switch (key)
        {
            // Audio
            case SettingsData.MuteAudioKey:
                return (T)(object)MuteAudio;
            case SettingsData.MusicVolumeKey:
                return (T)(object)MusicVolume;
            case SettingsData.SFXVolumeKey:
                return (T)(object)SFXVolume;
            case SettingsData.VoiceVolumeKey:
                return (T)(object)VoiceVolume;
            case SettingsData.VoicePercentageChanceKey:
                return (T)(object)VoicePercentageChance;

            // Video
            case SettingsData.FullscreenKey:
                return (T)(object)Fullscreen;
            case SettingsData.VSyncKey:
                return (T)(object)VSync;
            case SettingsData.TargetFrameRateKey:
                return (T)(object)TargetFrameRate;

            // Gameplay
            case SettingsData.InitialHandSizeKey:
                return (T)(object)InitialHandSize;
            case SettingsData.RandomisePlayerDeckKey:
                return (T)(object)RandomisePlayerDeck;
            case SettingsData.RandomiseDeckSizeKey:
                return (T)(object)RandomiseDeckSize;
            case SettingsData.SpyDrawAmountKey:
                return (T)(object)SpyDrawAmount;
            case SettingsData.LeaderCardEnabledKey:
                return (T)(object)LeaderCardEnabled;
            case SettingsData.FactionAbilityEnabledKey:
                return (T)(object)FactionAbilityEnabled;

            default:
                Debug.LogWarning($"[SettingsManager] Unknown setting key: {key}");
                return default;
        }
    }

    /// <summary>
    /// Resets all settings to their default values, saves, and applies them.
    /// </summary>
    public void ResetToDefaults()
    {
        // Audio
        MuteAudio = SettingsData.DefaultMuteAudio == 1;
        MusicVolume = SettingsData.DefaultMusicVolume;
        SFXVolume = SettingsData.DefaultSFXVolume;
        VoiceVolume = SettingsData.DefaultVoiceVolume;
        VoicePercentageChance = SettingsData.DefaultVoicePercentageChance;

        // Video
        Fullscreen = SettingsData.DefaultFullscreen == 1;
        VSync = SettingsData.DefaultVSync == 1;
        TargetFrameRate = SettingsData.DefaultTargetFrameRate;

        // Gameplay
        InitialHandSize = SettingsData.DefaultInitialHandSize;
        RandomisePlayerDeck = SettingsData.DefaultRandomisePlayerDeck == 1;
        RandomiseDeckSize = SettingsData.DefaultRandomiseDeckSize;
        SpyDrawAmount = SettingsData.DefaultSpyDrawAmount;
        LeaderCardEnabled = SettingsData.DefaultLeaderCardEnabled == 1;
        FactionAbilityEnabled = SettingsData.DefaultFactionAbilityEnabled == 1;

        // Save and apply
        SaveSettings();

        Debug.Log("[SettingsManager] All settings reset to default values.");
    }

    /// <summary>
    /// Converts an int stored in PlayerPrefs to a bool.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private bool GetBool(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue) == 1;
    }

    /// <summary>
    /// Sets a bool value in PlayerPrefs as an int.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}
