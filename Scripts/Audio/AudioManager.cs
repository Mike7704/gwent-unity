using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;   // For background music
    public AudioSource sfxSource;     // For UI clicks, card sounds, board sounds
    public AudioSource voiceSource;   // For character/voice lines like the logo

    [Header("Music Playlist")]
    public List<AudioClip> playlist = new List<AudioClip>();
    public bool shuffle = false;

    public delegate void OnTrackChanged(string trackName);
    public event OnTrackChanged TrackChanged;

    private int currentIndex = 0;
    private bool isPaused = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure sources exist
        if (!musicSource) musicSource = gameObject.AddComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
        if (!voiceSource) voiceSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = false;
        sfxSource.playOnAwake = false;
        voiceSource.playOnAwake = false;
    }

    void Start()
    {
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

    // Play a specific track
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.Play();

        // Fire event
        TrackChanged?.Invoke(clip.name);
    }

    // Play next track
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

    // Play previous track
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

    // Play a short SFX (UI, card, board)
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // Play a voice line (stops previous voice)
    public void PlayVoice(AudioClip clip)
    {
        if (voiceSource.isPlaying) voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    // Stop all audio
    public void StopAll()
    {
        musicSource.Stop();
        sfxSource.Stop();
        voiceSource.Stop();
    }
}
