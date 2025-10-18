using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Applies hover and click effects to buttons.
/// </summary>
[RequireComponent(typeof(Image))]
public class GwentButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Image image;
    private Material material;

    [Header("Hover Settings")]
    public float hoverBrightness = 1.1f;
    public float transitionSpeed = 8f;

    [Header("Sounds")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private Color baseColor;
    private bool hovering;

    void Awake()
    {
        image = GetComponent<Image>();

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
        if (hoverSound && AudioSystem.Instance != null)
        {
            AudioSystem.Instance.PlaySFX(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound && AudioSystem.Instance != null)
        {
            AudioSystem.Instance.PlaySFX(clickSound);
        }
    }
}
