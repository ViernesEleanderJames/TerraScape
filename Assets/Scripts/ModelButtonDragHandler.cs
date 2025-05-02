using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModelButtonDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler
{
    private bool isDragging = false;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = false; // Reset on pointer down
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true; // Started dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Optional: Handle visual dragging
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
        {
            // Only click if it wasn't a drag
            button.onClick.Invoke();
        }
        // If it was a drag, ignore click
    }
}
