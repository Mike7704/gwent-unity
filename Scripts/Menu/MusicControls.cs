using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the music control UI.
/// </summary>
public class MusicControlsUI : Singleton<MusicControlsUI>
{
    [Header("Controls")]
    public Button playPauseButton;
    public Button previousButton;
    public Button nextButton;
    public Button shuffleButton;
    public TextMeshProUGUI songNameText;
    public Slider volumeSlider;

    [Header("Sprites")]
    public Sprite playIcon;
    public Sprite pauseIcon;
    public Sprite shuffleOffIcon;
    public Sprite shuffleOnIcon;

    private bool isPlaying = true;

    void Start()
    {
        playPauseButton.onClick.AddListener(TogglePlayPause);
        previousButton.onClick.AddListener(PreviousTrack);
        nextButton.onClick.AddListener(NextTrack);
        shuffleButton.onClick.AddListener(ToggleShuffle);

        // Initialise buttons
        UpdatePlayPauseIcon();
        UpdateShuffleIcon();
        SetupMusicVolumeSlider();

        // Subscribe to track change event
        AudioSystem.Instance.TrackChanged += OnTrackChanged;
    }

    private void TogglePlayPause()
    {
        if (AudioSystem.Instance.musicSource.isPlaying)
        {
            AudioSystem.Instance.PauseMusic();
            isPlaying = false;
        }
        else
        {
            AudioSystem.Instance.ResumeMusic();
            isPlaying = true;
        }

        UpdatePlayPauseIcon();
    }

    private void PreviousTrack()
    {
        AudioSystem.Instance.PreviousTrack();
    }

    private void NextTrack()
    {
        AudioSystem.Instance.NextTrack();
    }

    private void ToggleShuffle()
    {
        AudioSystem.Instance.ToggleShuffle();
        UpdateShuffleIcon();
    }

    private void OnTrackChanged(string trackName)
    {
        if (songNameText != null)
            AudioSystem.Instance.GetCurrentTrackName();
    }

    private void SetupMusicVolumeSlider()
    {
        if (volumeSlider != null)
        {
            // Load saved volume or default to current music volume
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", AudioSystem.Instance.musicSource.volume);
            volumeSlider.value = savedVolume;
            AudioSystem.Instance.SetMusicVolume(savedVolume);

            // Update volume when slider changes and save it
            volumeSlider.onValueChanged.AddListener(value => {
                AudioSystem.Instance.SetMusicVolume(value);
            });
        }
    }

    private void UpdatePlayPauseIcon()
    {
        if (playPauseButton.image != null)
            playPauseButton.image.sprite = isPlaying ? pauseIcon : playIcon;
    }

    private void UpdateShuffleIcon()
    {
        if (shuffleButton.image != null)
            shuffleButton.image.sprite = AudioSystem.Instance.shuffle ? shuffleOnIcon : shuffleOffIcon;
    }
}
