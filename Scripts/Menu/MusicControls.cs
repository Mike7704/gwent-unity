using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [Header("Fade")]
    public CanvasGroup controlsCanvasGroup;
    public float visibleOpacity = 1f;
    public float fadedOpacity = 0.2f;
    public float fadeDelay = 1.5f;
    public float fadeDuration = 0.3f;

    private Coroutine fadeCoroutine;
    private bool isHoveringControls;

    private bool isPlaying = true;
    private const float SkipSeconds = 10f;

    void Start()
    {
        playPauseButton.onClick.AddListener(TogglePlayPause);
        SetupMouseClick(previousButton, PreviousTrack, SkipBackward);
        SetupMouseClick(nextButton, NextTrack, SkipForward);
        shuffleButton.onClick.AddListener(ToggleShuffle);

        // Initialise buttons
        UpdatePlayPauseIcon();
        UpdateShuffleIcon();
        SetupMusicVolumeSlider();

        SetupHoverEvents();

        // Subscribe to track change event
        AudioSystem.Instance.TrackChanged += OnTrackChanged;
    }

    private void SetupMouseClick(Button button, UnityEngine.Events.UnityAction left, UnityEngine.Events.UnityAction right)
    {
        // Remove default button click
        button.onClick.RemoveAllListeners();

        MouseClickInput input = button.GetComponent<MouseClickInput>();
        if (input == null)
            input = button.gameObject.AddComponent<MouseClickInput>();

        input.leftClick.RemoveAllListeners();
        input.rightClick.RemoveAllListeners();

        input.leftClick.AddListener(left);
        input.rightClick.AddListener(right);
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

    private void SkipForward()
    {
        AudioSystem.Instance.SkipMusicForward(SkipSeconds);
    }

    private void SkipBackward()
    {
        AudioSystem.Instance.SkipMusicBackward(SkipSeconds);
    }

    private void ToggleShuffle()
    {
        AudioSystem.Instance.ToggleShuffle();
        UpdateShuffleIcon();
    }

    private void OnTrackChanged(string trackName)
    {
        if (songNameText != null)
            songNameText.text = AudioSystem.Instance.GetCurrentTrackName();
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

    // -------------------------
    // Hover events to fade controls
    // -------------------------

    private void SetupHoverEvents()
    {
        AddHoverTrigger(playPauseButton.gameObject);
        AddHoverTrigger(previousButton.gameObject);
        AddHoverTrigger(nextButton.gameObject);
        AddHoverTrigger(shuffleButton.gameObject);
        AddHoverTrigger(volumeSlider.gameObject);

        fadeCoroutine = StartCoroutine(FadeOutAfterDelay());
    }

    private void AddHoverTrigger(GameObject target)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = target.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        enterEntry.callback.AddListener(_ => OnControlsHoverEnter());
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        exitEntry.callback.AddListener(_ => OnControlsHoverExit());
        trigger.triggers.Add(exitEntry);
    }

    private void OnControlsHoverEnter()
    {
        isHoveringControls = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeControls(visibleOpacity));
    }

    private void OnControlsHoverExit()
    {
        isHoveringControls = false;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutAfterDelay());
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(fadeDelay);

        if (!isHoveringControls)
            yield return FadeControls(fadedOpacity);
    }

    private IEnumerator FadeControls(float targetOpacity)
    {
        if (controlsCanvasGroup == null)
            yield break;

        float startOpacity = controlsCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            controlsCanvasGroup.alpha = Mathf.Lerp(startOpacity, targetOpacity, elapsed / fadeDuration);
            yield return null;
        }

        controlsCanvasGroup.alpha = targetOpacity;
    }
}
