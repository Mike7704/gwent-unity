using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicControlsUI : MonoBehaviour
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

        // Initialise button images
        UpdatePlayPauseIcon();
        UpdateShuffleIcon();

        // Subscribe to track change event
        AudioManager.Instance.TrackChanged += OnTrackChanged;

        // Volume slider setup
        if (volumeSlider != null)
        {
            // Load saved volume or default to current music volume
            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", AudioManager.Instance.musicSource.volume);
            volumeSlider.value = savedVolume;
            AudioManager.Instance.musicSource.volume = savedVolume;

            // Update volume when slider changes and save it
            volumeSlider.onValueChanged.AddListener(value => {
                AudioManager.Instance.musicSource.volume = value;
                PlayerPrefs.SetFloat("MusicVolume", value);
            });
        }
    }

    void TogglePlayPause()
    {
        if (AudioManager.Instance.musicSource.isPlaying)
        {
            AudioManager.Instance.PauseMusic();
            isPlaying = false;
        }
        else
        {
            AudioManager.Instance.ResumeMusic();
            isPlaying = true;
        }

        UpdatePlayPauseIcon();
    }

    void PreviousTrack()
    {
        AudioManager.Instance.PreviousTrack();
    }

    void NextTrack()
    {
        AudioManager.Instance.NextTrack();
    }

    void ToggleShuffle()
    {
        AudioManager.Instance.ToggleShuffle();
        UpdateShuffleIcon();
    }

    void OnTrackChanged(string trackName)
    {
        if (songNameText != null)
            songNameText.text = trackName;
    }

    void SetVolume(float value)
    {
        AudioManager.Instance.musicSource.volume = value;
    }

    void UpdatePlayPauseIcon()
    {
        if (playPauseButton.image != null)
            playPauseButton.image.sprite = isPlaying ? pauseIcon : playIcon;
    }

    void UpdateShuffleIcon()
    {
        if (shuffleButton.image != null)
            shuffleButton.image.sprite = AudioManager.Instance.shuffle ? shuffleOnIcon : shuffleOffIcon;
    }
}
