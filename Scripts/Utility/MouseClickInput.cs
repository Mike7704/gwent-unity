using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MouseClickInput : MonoBehaviour, IPointerClickHandler
{

    public UnityEvent leftClick = new();
    public UnityEvent middleClick = new();
    public UnityEvent rightClick = new();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            leftClick?.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Middle)
            middleClick?.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right)
            rightClick?.Invoke();
    }
}