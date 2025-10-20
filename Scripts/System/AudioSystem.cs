using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio system managing background music, sound effects, and voice lines.
/// </summary>
public class AudioSystem : Singleton<AudioSystem>
{
    [Header("Audio Sources")]
    public AudioSource musicSource;   // For background music
    public AudioSource sfxSource;     // For UI clicks, card sounds, board sounds
    public AudioSource voiceSource;   // For character/voice lines like the logo

    [Header("Music Playlist")]
    public List<AudioClip> playlist = new List<AudioClip>();
    public bool shuffle = false;

    public delegate void OnTrackChanged(string trackName);
    public event OnTrackChanged TrackChanged;

    private bool initialised = false;
    private int currentIndex = 0;
    private bool isPaused = false;
    private int voicePercentageChance = 100;

    public void Initialise()
    {
        if (initialised) return;
        initialised = true;

        // Ensure sources exist
        if (!musicSource) musicSource = gameObject.AddComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
        if (!voiceSource) voiceSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = false;
        sfxSource.playOnAwake = false;
        voiceSource.playOnAwake = false;

        if (playlist.Count > 0)
        {
            currentIndex = Random.Range(0, playlist.Count);
            PlayMusic(playlist[currentIndex]);
            isPaused = false;
        }
    }

    void Update()
    {
        if (!Application.isFocused) return; // Skip updates if Unity is not focused

        // Auto-play next track when current finishes
        if (!musicSource.isPlaying && !isPaused && playlist.Count > 0 && musicSource.clip != null)
        {
            NextTrack();
        }
    }

    /// <summary>
    /// Plays the given music clip.
    /// </summary>
    /// <param name="clip"></param>
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.Play();

        // Fire event
        TrackChanged?.Invoke(clip.name);
    }

    /// <summary>
    /// Plays the next track in the playlist.
    /// </summary>
    public void NextTrack()
    {
        if (playlist.Count == 0) return;

        if (shuffle)
        {
            currentIndex = Random.Range(0, playlist.Count);
        }
        else
        {
            currentIndex = (currentIndex + 1) % playlist.Count;
        }

        PlayMusic(playlist[currentIndex]);
    }

    /// <summary>
    /// Plays the previous track in the playlist.
    /// </summary>
    public void PreviousTrack()
    {
        if (playlist.Count == 0) return;

        if (shuffle)
        {
            currentIndex = Random.Range(0, playlist.Count);
        }
        else
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = playlist.Count - 1;
        }

        PlayMusic(playlist[currentIndex]);
    }

    public void ToggleShuffle()
    {
        shuffle = !shuffle;
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
            isPaused = true;
        }
    }

    public void ResumeMusic()
    {
        if (isPaused && musicSource.clip != null)
        {
            musicSource.Play();
            isPaused = false;
        }
    }

    public string GetCurrentTrackName()
    {
        return (musicSource.clip != null) ? musicSource.clip.name : "";
    }

    public void SetMuteAudio(bool mute)
    {
        musicSource.mute = mute;
        sfxSource.mute = mute;
        voiceSource.mute = mute;
    }

    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
    }

    public void SetVoiceVolume(float value)
    {
        voiceSource.volume = value;
    }

    public void SetVoicePercentageChance(int value)
    {
        voicePercentageChance = Mathf.Clamp(value, 0, 100);
    }

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Plays a voice line and stops any currently playing voice.
    /// </summary>
    /// <param name="clip"></param>
    public void PlayVoice(AudioClip clip)
    {
        // Check percentage chance for playing a voice line
        if (voicePercentageChance < RandomUtils.GetRandom(0, 100)) return;

        if (voiceSource.isPlaying) voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    /// <summary>
    /// Stops all audio playback.
    /// </summary>
    public void StopAll()
    {
        musicSource.Stop();
        sfxSource.Stop();
        voiceSource.Stop();
    }
}
