using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image), typeof(AudioSource))]
public class GwentLogo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Image image;
    private AudioSource audioSource;
    private Material material;

    [Header("Hover Settings")]
    public float hoverBrightness = 1.1f;
    public float transitionSpeed = 8f;

    [Header("Sounds")]
    public AudioClip hoverSound;
    public AudioClip[] clickSounds;

    private Color baseColor;
    private bool hovering;

    void Awake()
    {
        image = GetComponent<Image>();
        audioSource = GetComponent<AudioSource>();

        // Use a copy of the material so multiple buttons don't share it
        material = Instantiate(image.material);
        image.material = material;

        baseColor = material.color; // save original color
    }

    void Update()
    {
        // Smoothly animate color brightness
        float targetBrightness = hovering ? hoverBrightness : 1f;
        Color targetColor = baseColor * targetBrightness; // multiply RGB by brightness
        material.color = Color.Lerp(material.color, targetColor, Time.deltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        if (hoverSound) audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSounds != null && clickSounds.Length > 0)
        {
            // Stop any currently playing sound
            if (audioSource.isPlaying) audioSource.Stop();

            // Play a random voice line
            int randomIndex = Random.Range(0, clickSounds.Length);
            audioSource.PlayOneShot(clickSounds[randomIndex]);
        }
    }
}
